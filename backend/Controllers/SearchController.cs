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
        private readonly IConfiguration _configuration;

        public SearchController(KorpusDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        private bool IsAuthorized()
        {
            var required = _configuration["AccessPassphrase"];
            if (string.IsNullOrEmpty(required))
            {
                // No passphrase required
                return true;
            }

            if (Request.Headers.TryGetValue("X-Access-Passphrase", out var provided))
            {
                return provided == required;
            }

            return false;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Search(
            [FromQuery] string? q,
            [FromQuery] int? messageSequence,
            [FromQuery] int? messageSequenceMin,
            [FromQuery] int? messageSequenceMax,
            [FromQuery] int? maxPerImportBatch,
            [FromQuery] bool includeAll = false)
        {
            if (!IsAuthorized())
                return Unauthorized(new { error = "Invalid or missing passphrase" });

            var queryText = q ?? string.Empty;

            if (!includeAll && string.IsNullOrWhiteSpace(queryText))
                return BadRequest(new { error = "Query parameter 'q' is required." });

            if (messageSequenceMin.HasValue && messageSequenceMin <= 0)
                return BadRequest(new { error = "Parameter 'messageSequenceMin' must be greater than 0." });

            if (messageSequenceMax.HasValue && messageSequenceMax <= 0)
                return BadRequest(new { error = "Parameter 'messageSequenceMax' must be greater than 0." });

            if (!messageSequence.HasValue && messageSequenceMin.HasValue && messageSequenceMax.HasValue && messageSequenceMin > messageSequenceMax)
                return BadRequest(new { error = "'messageSequenceMin' cannot be greater than 'messageSequenceMax'." });

            if (maxPerImportBatch.HasValue && maxPerImportBatch <= 0)
                return BadRequest(new { error = "Parameter 'maxPerImportBatch' must be greater than 0." });

            var messageQuery = _dbContext.Messages
                .Include(m => m.Conversation)
                .AsQueryable();

            if (!includeAll)
            {
                var loweredQuery = queryText.ToLower();
                messageQuery = messageQuery.Where(m => m.Content.ToLower().Contains(loweredQuery) || m.Author.ToLower().Contains(loweredQuery));
            }

            if (messageSequence.HasValue)
            {
                messageQuery = messageQuery.Where(m => m.Sequence == messageSequence.Value);
            }
            else
            {
                if (messageSequenceMin.HasValue)
                {
                    messageQuery = messageQuery.Where(m => m.Sequence >= messageSequenceMin.Value);
                }

                if (messageSequenceMax.HasValue)
                {
                    messageQuery = messageQuery.Where(m => m.Sequence <= messageSequenceMax.Value);
                }
            }

            var rawResults = await messageQuery
                .Select(m => new {
                    conversationId = m.ConversationId,
                    conversationTitle = m.Conversation.Title,
                    messageId = m.Id,
                    author = m.Author,
                    content = m.Content,
                    createTime = m.CreateTime,
                    sequence = m.Sequence,
                    importBatchId = m.Conversation.ImportBatchId
                })
                .ToListAsync();

            var results = rawResults;

            if (maxPerImportBatch.HasValue)
            {
                results = rawResults
                    .GroupBy(r => r.importBatchId ?? string.Empty)
                    .SelectMany(group => group
                        .OrderBy(_ => Guid.NewGuid())
                        .Take(maxPerImportBatch.Value))
                    .ToList();
            }

            // Calculate statistics
            var totalMatches = results.Count;
            var agentMatches = results.Count(r => string.Equals(r.author, "assistant", StringComparison.OrdinalIgnoreCase));
            var userMatches = results.Count(r => string.Equals(r.author, "user", StringComparison.OrdinalIgnoreCase));
            var otherMatches = Math.Max(0, totalMatches - agentMatches - userMatches);
            var uniqueMessages = results.Select(r => r.messageId).Distinct().Count();

            int wordOccurrences = 0;
            if (!includeAll && !string.IsNullOrWhiteSpace(queryText))
            {
                var loweredQuery = queryText.ToLower();
                wordOccurrences = results.Sum(r =>
                    r.content.ToLower().Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                        .Count(word => word.Contains(loweredQuery)));
            }
            
            // Get total message count from corpus
            var totalMessagesInCorpus = await _dbContext.Messages.CountAsync();

            return Ok(new { 
                results = results.Select(r => new {
                    r.conversationId,
                    r.conversationTitle,
                    r.messageId,
                    r.author,
                    r.content,
                    r.createTime,
                    r.sequence
                }),
                stats = new {
                    totalMatches,
                    agentMatches,
                    userMatches,
                    uniqueMessages,
                    wordOccurrences,
                    otherMatches,
                    totalMessagesInCorpus
                }
            });
        }

        [HttpGet]
        [Route("stats")]
        public async Task<IActionResult> Stats()
        {
            if (!IsAuthorized())
                return Unauthorized(new { error = "Invalid or missing passphrase" });

            var conversationCount = await _dbContext.Conversations.CountAsync();
            var messageCount = await _dbContext.Messages.CountAsync();
            var uploadCount = await _dbContext.Conversations.Select(c => c.ImportBatchId).Distinct().CountAsync();

            return Ok(new {
                conversations = conversationCount,
                messages = messageCount,
                uploads = uploadCount
            });
        }
    }
}
