using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using ChatGPTcorpus.Models;

namespace ChatGPTcorpus.Services
{
    public class ZipService
    {
        public void ExtractZip(string zipPath, string extractPath)
        {
            Console.WriteLine($"Attempting to extract ZIP file from: {zipPath}");
            
            if (!File.Exists(zipPath))
            {
                throw new FileNotFoundException($"ZIP file not found: {zipPath}");
            }

            try
            {
                // Create the extraction directory if it doesn't exist
                Directory.CreateDirectory(extractPath);
                
                Console.WriteLine($"Extracting to: {extractPath}");
                ZipFile.ExtractToDirectory(zipPath, extractPath);

                // Verify the conversations.json file exists
                var conversationsFile = Path.Combine(extractPath, "conversations.json");
                if (!File.Exists(conversationsFile))
                {
                    throw new FileNotFoundException("No conversations.json file found in the ZIP archive");
                }

                // Move conversations.json to Data/RawConversations/{userId}/conversations.json
                var userId = Path.GetFileName(extractPath);
                var rawConvDir = Path.Combine(Directory.GetCurrentDirectory(), "Data", "RawConversations", userId);
                Directory.CreateDirectory(rawConvDir);
                var destConvFile = Path.Combine(rawConvDir, "conversations.json");
                if (File.Exists(destConvFile)) File.Delete(destConvFile);
                File.Move(conversationsFile, destConvFile);

                // Delete the rest of the extracted files and the extraction directory
                Directory.Delete(extractPath, true);

                Console.WriteLine($"Moved conversations.json to {destConvFile} and cleaned up extraction directory.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError during extraction: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        // Keep the old method for backward compatibility
        public List<Conversation> ExtractAndLoadConversations(string zipPath)
        {
            Console.WriteLine($"Attempting to load ZIP file from: {zipPath}");
            
            if (!File.Exists(zipPath))
            {
                throw new FileNotFoundException($"ZIP file not found: {zipPath}");
            }

            var conversations = new List<Conversation>();
            var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Console.WriteLine($"Created temporary directory: {tempPath}");

            try
            {
                Directory.CreateDirectory(tempPath);
                Console.WriteLine("Extracting ZIP file...");
                ZipFile.ExtractToDirectory(zipPath, tempPath);

                Console.WriteLine("Searching for JSON files...");
                var jsonFiles = Directory.GetFiles(tempPath, "*.json", SearchOption.AllDirectories);
                Console.WriteLine($"Found {jsonFiles.Length} JSON files:");
                foreach (var file in jsonFiles)
                {
                    Console.WriteLine($"- {Path.GetFileName(file)}");
                }

                var conversationsFile = jsonFiles.FirstOrDefault(f => 
                    string.Equals(Path.GetFileName(f), "conversations.json", StringComparison.OrdinalIgnoreCase));

                if (conversationsFile == null)
                {
                    throw new FileNotFoundException("No conversations.json file found in the ZIP archive");
                }

                Console.WriteLine($"\nFound conversations file: {conversationsFile}");
                Console.WriteLine("Reading file contents...");
                var jsonContent = File.ReadAllText(conversationsFile);
                Console.WriteLine($"File size: {jsonContent.Length} bytes");

                Console.WriteLine("\nDeserializing JSON...");
                conversations = JsonSerializer.Deserialize<List<Conversation>>(jsonContent) ?? new List<Conversation>();
                
                Console.WriteLine($"\nSuccessfully loaded {conversations.Count} conversations");
                Console.WriteLine("\nSample of first few conversations:");
                foreach (var conv in conversations.Take(3))
                {
                    Console.WriteLine($"\nTitle: {conv.Title}");
                    Console.WriteLine($"Number of messages: {conv.Messages.Count}");
                    if (conv.Messages.Any())
                    {
                        var firstMsg = conv.Messages.Values.First();
                        Console.WriteLine($"First message author: {firstMsg.Author}");
                        Console.WriteLine($"First message preview: {firstMsg.Content.Substring(0, Math.Min(100, firstMsg.Content.Length))}...");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError during processing: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
            finally
            {
                if (Directory.Exists(tempPath))
                {
                    Console.WriteLine($"\nCleaning up temporary directory: {tempPath}");
                    Directory.Delete(tempPath, true);
                }
            }

            return conversations;
        }
    }
} 