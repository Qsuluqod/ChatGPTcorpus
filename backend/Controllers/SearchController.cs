using Microsoft.AspNetCore.Mvc;
using ChatGPTcorpus.Models;
using ChatGPTcorpus.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ChatGPTcorpus.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly KorpusDbContext _dbContext;

        public SearchController(KorpusDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(new { error = "Query parameter 'q' is required." });

            var results = await _dbContext.Messages
                .Include(m => m.Conversation)
                .Where(m => m.Content.ToLower().Contains(q.ToLower()) || m.Author.ToLower().Contains(q.ToLower()))
                .Select(m => new {
                    conversationId = m.ConversationId,
                    conversationTitle = m.Conversation.Title,
                    messageId = m.Id,
                    author = m.Author,
                    content = m.Content,
                    createTime = m.CreateTime
                })
                .ToListAsync();

            // Calculate statistics
            var totalMatches = results.Count;
            var agentMatches = results.Count(r => r.author.ToLower() == "assistant");
            var userMatches = results.Count(r => r.author.ToLower() == "user");
            var uniqueMessages = results.Select(r => r.messageId).Distinct().Count();
            var wordOccurrences = results.Sum(r => 
                r.content.ToLower().Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                    .Count(word => word.Contains(q.ToLower())));
            
            // Get total message count from corpus
            var totalMessagesInCorpus = await _dbContext.Messages.CountAsync();

            return Ok(new { 
                results,
                stats = new {
                    totalMatches,
                    agentMatches,
                    userMatches,
                    uniqueMessages,
                    wordOccurrences,
                    totalMessagesInCorpus
                }
            });
        }

        [HttpGet]
        [Route("stats")]
        public async Task<IActionResult> Stats()
        {
            var contributionCount = await _dbContext.Conversations.Select(c => c.Author).Distinct().CountAsync();
            var conversationCount = await _dbContext.Conversations.CountAsync();
            var messageCount = await _dbContext.Messages.CountAsync();

            return Ok(new {
                contributions = contributionCount,
                conversations = conversationCount,
                messages = messageCount
            });
        }
    }
} 