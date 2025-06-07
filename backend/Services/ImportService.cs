using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ChatGPTcorpus.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace ChatGPTcorpus.Services
{
    public class ImportService
    {
        private readonly string _basePath;

        public ImportService(string basePath)
        {
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
        }

        public async Task<List<Conversation>> ImportConversationsAsync(string userId, Dictionary<string, object> metadata = null)
        {
            try
            {
                var conversations = new List<Conversation>();
                var jsonPath = Path.Combine(_basePath, "RawConversations", userId, "conversations.json");
                
                Console.WriteLine($"Attempting to read conversations from: {jsonPath}");
                
                if (!File.Exists(jsonPath))
                {
                    throw new FileNotFoundException($"conversations.json not found at path: {jsonPath}");
                }

                var json = await File.ReadAllTextAsync(jsonPath);
                Console.WriteLine($"Successfully read JSON file, size: {json.Length} bytes");

                // First try to parse as JArray to validate JSON structure
                var jsonArray = JArray.Parse(json);
                Console.WriteLine($"Successfully parsed JSON array with {jsonArray.Count} items");

                // Convert each conversation object
                foreach (var item in jsonArray)
                {
                    try
                    {
                        var conversation = ParseConversation(item as JObject);
                        if (conversation != null)
                        {
                            conversations.Add(conversation);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing conversation: {ex.Message}");
                        // Continue with next conversation instead of failing completely
                        continue;
                    }
                }

                Console.WriteLine($"Successfully parsed {conversations.Count} conversations");

                if (metadata != null)
                {
                    foreach (var conv in conversations)
                    {
                        try
                        {
                            conv.IsSingleUser = metadata.ContainsKey("isSingleUser") ? Convert.ToBoolean(metadata["isSingleUser"]) : false;
                            conv.Gender = metadata.ContainsKey("gender") ? metadata["gender"]?.ToString() ?? string.Empty : string.Empty;
                            conv.AgeCategory = metadata.ContainsKey("ageCategory") ? metadata["ageCategory"]?.ToString() ?? string.Empty : string.Empty;
                            conv.EducationLevel = metadata.ContainsKey("educationLevel") ? metadata["educationLevel"]?.ToString() ?? string.Empty : string.Empty;
                            conv.CurrentRegion = metadata.ContainsKey("currentRegion") ? metadata["currentRegion"]?.ToString() ?? string.Empty : string.Empty;
                            conv.ChildhoodRegion = metadata.ContainsKey("childhoodRegion") ? metadata["childhoodRegion"]?.ToString() ?? string.Empty : string.Empty;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error applying metadata to conversation {conv.Id}: {ex.Message}");
                            // Continue with next conversation
                            continue;
                        }
                    }
                }

                // Print debug information
                PrintDebugInfo(conversations);

                return conversations;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ImportConversationsAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private void PrintDebugInfo(List<Conversation> conversations)
        {
            Console.WriteLine("\n=== Debug Information ===");
            Console.WriteLine($"Total conversations loaded: {conversations.Count}");
            
            // Print first 3 conversations
            foreach (var conv in conversations.Take(3))
            {
                Console.WriteLine($"\nConversation: {conv.Title}");
                Console.WriteLine($"ID: {conv.Id}");
                Console.WriteLine($"Created: {conv.CreateTime}");
                Console.WriteLine($"Number of messages: {conv.Messages.Count}");
                
                // Print first 3 messages
                var firstMessages = conv.Messages.Values.Take(3).ToList();
                for (int i = 0; i < firstMessages.Count; i++)
                {
                    var msg = firstMessages[i];
                    Console.WriteLine($"\nMessage {i + 1}:");
                    Console.WriteLine($"Author: {msg.Author}");
                    Console.WriteLine($"Content preview: {msg.Content.Substring(0, Math.Min(100, msg.Content.Length))}...");
                }
                Console.WriteLine("\n" + new string('-', 50));
            }
        }

        private Conversation ParseConversation(JObject conversationObj)
        {
            if (conversationObj == null) return null;

            try
            {
                var conversation = new Conversation
                {
                    Id = conversationObj["id"]?.ToString() ?? Guid.NewGuid().ToString(),
                    Title = conversationObj["title"]?.ToString() ?? "Untitled Conversation",
                    CreateTime = conversationObj["create_time"]?.Value<long>() != null 
                        ? DateTimeOffset.FromUnixTimeSeconds(conversationObj["create_time"].Value<long>()).DateTime 
                        : DateTime.UtcNow,
                    UpdateTime = DateTime.UtcNow,
                    Messages = new Dictionary<string, Message>(),
                    Author = conversationObj["author"]?["role"]?.ToString() ?? string.Empty
                };

                var mapping = conversationObj["mapping"] as JObject;
                if (mapping == null) return conversation;

                // First pass: Create all message nodes and messages
                foreach (var node in mapping.Properties())
                {
                    var nodeObj = node.Value as JObject;
                    if (nodeObj == null) continue;

                    var messageObj = nodeObj["message"] as JObject;
                    if (messageObj != null)
                    {
                        // Safely parse create_time
                        DateTime msgCreateTime = DateTime.UtcNow;
                        var createTimeToken = messageObj["create_time"];
                        if (createTimeToken != null && createTimeToken.Type != JTokenType.Null && createTimeToken.Type == JTokenType.Integer)
                        {
                            msgCreateTime = DateTimeOffset.FromUnixTimeSeconds(createTimeToken.Value<long>()).DateTime;
                        }

                        var message = new Message
                        {
                            Id = messageObj["id"]?.ToString() ?? node.Name,
                            Author = messageObj["author"]?["role"]?.ToString() ?? "unknown",
                            Content = ExtractMessageContent(messageObj),
                            CreateTime = msgCreateTime,
                            Status = messageObj["status"]?.ToString() ?? "complete",
                            Weight = messageObj["weight"]?.Value<int>() ?? 1,
                            EndTurn = ParseEndTurn(messageObj["end_turn"]),
                            Recipient = messageObj["recipient"]?.ToString() ?? string.Empty
                        };

                        // Only add message if it has content and author
                        if (!string.IsNullOrWhiteSpace(message.Content) && !string.IsNullOrWhiteSpace(message.Author))
                        {
                            conversation.Messages[message.Id] = message;
                        }
                    }
                }

                return conversation;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing conversation object: {ex.Message}");
                return null;
            }
        }

        private string ExtractMessageContent(JObject messageObj)
        {
            try
            {
                var content = messageObj["content"];
                if (content == null) return string.Empty;

                // Handle different content formats
                if (content is JObject contentObj)
                {
                    var parts = contentObj["parts"] as JArray;
                    if (parts != null && parts.Any())
                    {
                        return string.Join("\n", parts.Select(p => p.ToString()));
                    }
                }
                else if (content is JArray contentArray)
                {
                    return string.Join("\n", contentArray.Select(p => p.ToString()));
                }
                else if (content is JValue contentValue)
                {
                    return contentValue.ToString();
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting message content: {ex.Message}");
                return string.Empty;
            }
        }

        private object ParseEndTurn(JToken endTurnToken)
        {
            if (endTurnToken == null || endTurnToken.Type == JTokenType.Null)
                return null;
            if (endTurnToken.Type == JTokenType.Boolean)
                return endTurnToken.Value<bool>();
            if (endTurnToken.Type == JTokenType.Array)
                return endTurnToken.ToObject<List<object>>();
            return endTurnToken.ToString();
        }
    }
}