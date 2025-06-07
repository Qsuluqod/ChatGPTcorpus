using Microsoft.AspNetCore.Mvc;
using ChatGPTcorpus.Services;
using ChatGPTcorpus.Models;
using System;
using System.Linq;
using System.IO;

namespace ChatGPTcorpus.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly SearchServiceProvider _searchProvider;

        public SearchController(SearchServiceProvider searchProvider)
        {
            _searchProvider = searchProvider;
        }

        [HttpGet]
        [Route("")]
        public IActionResult Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(new { error = "Query parameter 'q' is required." });

            var conversations = _searchProvider.Conversations;
            if (conversations == null || !conversations.Any())
                return Ok(new { results = Array.Empty<object>(), message = "No conversations loaded." });

            var searchService = new SearchService(conversations);
            var results = searchService.Search(q)
                .Select(r => new {
                    conversationId = r.Conversation.Id,
                    conversationTitle = r.Conversation.Title,
                    messageId = r.Message.Id,
                    author = r.Message.Author,
                    content = r.Message.Content,
                    createTime = r.Message.CreateTime
                })
                .ToList();

            return Ok(new { results });
        }

        [HttpGet]
        [Route("stats")]
        public IActionResult Stats()
        {
            string rawConvPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "RawConversations");
            int contributionCount = Directory.Exists(rawConvPath) ? Directory.GetDirectories(rawConvPath).Length : 0;
            int conversationCount = 0;
            int messageCount = 0;

            if (Directory.Exists(rawConvPath))
            {
                var dirs = Directory.GetDirectories(rawConvPath);
                foreach (var dir in dirs)
                {
                    var convFile = Path.Combine(dir, "conversations.json");
                    if (System.IO.File.Exists(convFile))
                    {
                        try
                        {
                            var json = System.IO.File.ReadAllText(convFile);
                            var conversations = System.Text.Json.JsonSerializer.Deserialize<List<ChatGPTcorpus.Models.Conversation>>(json);
                            if (conversations != null)
                            {
                                conversationCount += conversations.Count;
                                foreach (var conv in conversations)
                                {
                                    int msgCount = conv.Messages != null ? conv.Messages.Count : 0;
                                    messageCount += msgCount;
                                    Console.WriteLine($"Conversation: {conv.Title}, Messages: {msgCount}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to read {convFile}: {ex.Message}");
                        }
                    }
                }
            }

            Console.WriteLine($"Stats endpoint called - Contributions: {contributionCount}, Conversations: {conversationCount}, Messages: {messageCount}");
            return Ok(new {
                contributions = contributionCount,
                conversations = conversationCount,
                messages = messageCount
            });
        }
    }
} 