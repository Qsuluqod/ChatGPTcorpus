using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ChatGPTcorpus.Models;
using ChatGPTcorpus.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace ChatGPTcorpus.Services
{
    public class ImportService
    {
        private readonly string _basePath;
        private readonly KorpusDbContext _dbContext;

        public ImportService(string basePath, KorpusDbContext dbContext)
        {
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<List<Conversation>> ImportConversationsAsync(string userId, Dictionary<string, object>? metadata = null)
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
                            
                            // Convert to database model and save
                            var dbConversation = ConvertToDbModel(conversation, metadata ?? new Dictionary<string, object>());
                            await SaveConversationToDatabaseAsync(dbConversation);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing conversation: {ex.Message}");
                        // Continue with next conversation instead of failing completely
                        continue;
                    }
                }

                Console.WriteLine($"Successfully parsed and saved {conversations.Count} conversations");

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

        private async Task SaveConversationToDatabaseAsync(DbConversation conversation)
        {
            try
            {
                // Check if conversation already exists
                var existingConversation = await _dbContext.Conversations
                    .Include(c => c.Messages)
                    .FirstOrDefaultAsync(c => c.Id == conversation.Id);

                if (existingConversation != null)
                {
                    // Update existing conversation
                    _dbContext.Entry(existingConversation).CurrentValues.SetValues(conversation);
                    
                    // Remove existing messages
                    _dbContext.Messages.RemoveRange(existingConversation.Messages);
                }
                else
                {
                    // Add new conversation
                    await _dbContext.Conversations.AddAsync(conversation);
                }

                // Save changes
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving conversation to database: {ex.Message}");
                throw;
            }
        }

        private static T? GetValueFromMetadata<T>(Dictionary<string, object>? metadata, string key, T? defaultValue = default)
        {
            if (metadata == null || !metadata.ContainsKey(key) || metadata[key] == null)
                return defaultValue;

            var value = metadata[key];

            if (value is T tValue)
                return tValue;

            if (value is System.Text.Json.JsonElement jsonElement)
            {
                try
                {
                    if (typeof(T) == typeof(bool) && (jsonElement.ValueKind == System.Text.Json.JsonValueKind.True || jsonElement.ValueKind == System.Text.Json.JsonValueKind.False))
                        return (T)(object)jsonElement.GetBoolean();
                    if (typeof(T) == typeof(string)) {
                        var val =  jsonElement.GetString() ?? "";
                        return (T)(object)val;
                    }
                    if (typeof(T) == typeof(int) && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Number)
                        return (T)(object)jsonElement.GetInt32();
                }
                catch { }
            }

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        private static DateTime ToUtc(DateTime dt)
        {
            if (dt.Kind == DateTimeKind.Utc)
                return dt;
            if (dt.Kind == DateTimeKind.Local)
                return dt.ToUniversalTime();
            // Unspecified
            return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        }

        private DbConversation ConvertToDbModel(Conversation conversation, Dictionary<string, object> metadata)
        {
            var dbConversation = new DbConversation
            {
                Id = conversation.Id,
                Title = conversation.Title,
                CreateTime = ToUtc(conversation.CreateTime),
                UpdateTime = ToUtc(conversation.UpdateTime),
                Author = conversation.Author,
                IsSingleUser = conversation.IsSingleUser,
                Gender = conversation.Gender,
                AgeCategory = conversation.AgeCategory,
                EducationLevel = conversation.EducationLevel,
                CurrentRegion = conversation.CurrentRegion,
                ChildhoodRegion = conversation.ChildhoodRegion,
                Messages = conversation.Messages.Values.Select(m => new DbMessage
                {
                    Id = m.Id,
                    Author = m.Author,
                    CreateTime = ToUtc(m.CreateTime),
                    Content = m.Content,
                    Status = m.Status,
                    EndTurn = m.EndTurn?.ToString(),
                    Weight = m.Weight,
                    Metadata = m.Metadata,
                    Recipient = m.Recipient,
                    ConversationId = conversation.Id
                }).ToList()
            };

            // Apply metadata if provided
            if (metadata != null)
            {
                dbConversation.IsSingleUser = GetValueFromMetadata(metadata, "isSingleUser", false);
                dbConversation.Gender = GetValueFromMetadata(metadata, "gender", "");
                dbConversation.AgeCategory = GetValueFromMetadata(metadata, "ageCategory", "");
                dbConversation.EducationLevel = GetValueFromMetadata(metadata, "educationLevel", "");
                dbConversation.CurrentRegion = GetValueFromMetadata(metadata, "currentRegion", "");
                dbConversation.ChildhoodRegion = GetValueFromMetadata(metadata, "childhoodRegion", "");
            }

            return dbConversation;
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

        private Conversation? ParseConversation(JObject? conversationObj)
        {
            if (conversationObj == null) return null;

            try
            {
                var conversation = new Conversation
                {
                    Id = conversationObj["id"]?.ToString() ?? Guid.NewGuid().ToString(),
                    Title = conversationObj["title"]?.ToString() ?? "Untitled Conversation",
                    CreateTime = conversationObj["create_time"] is JValue v && v.Type == JTokenType.Integer && v.Value != null
                        ? DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(v.Value ?? 0)).DateTime
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
                        return string.Join("\n", parts.Select(p => p?.ToString() ?? string.Empty));
                    }
                }
                else if (content is JArray contentArray)
                {
                    return string.Join("\n", contentArray.Select(p => p?.ToString() ?? string.Empty));
                }
                else if (content is JValue contentValue)
                {
                    return contentValue?.ToString() ?? string.Empty;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting message content: {ex.Message}");
                return string.Empty;
            }
        }

        private object? ParseEndTurn(JToken? endTurnToken)
        {
            if (endTurnToken == null || endTurnToken.Type == JTokenType.Null)
                return null;
            if (endTurnToken.Type == JTokenType.Boolean)
                return endTurnToken.Value<bool>();
            if (endTurnToken.Type == JTokenType.Array)
                return endTurnToken.ToObject<List<object>>();
            return endTurnToken?.ToString() ?? string.Empty;
        }

        private long? SafeValueLong(IEnumerable<JToken>? value)
        {
            if (value == null) return null;
            var first = value.FirstOrDefault();
            return first != null ? first.Value<long?>() : null;
        }
    }
}