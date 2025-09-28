using Microsoft.AspNetCore.Mvc;
using ChatGPTcorpus.Models;
using ChatGPTcorpus.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatGPTcorpus.Controllers
{
    [ApiController]
    [Route("api/conversations")]
    public class ConversationController : ControllerBase
    {
        private readonly KorpusDbContext _dbContext;

        public ConversationController(KorpusDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("{conversationId}")]
        public async Task<IActionResult> GetConversation(string conversationId, [FromQuery] int? messageSequence)
        {
            var conversation = await _dbContext.Conversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null)
                return NotFound(new { error = "Conversation not found." });

            IEnumerable<DbMessage> messagesQuery = conversation.Messages
                .OrderBy(m => m.Sequence)
                .ThenBy(m => m.CreateTime);

            var highlightedMessage = messageSequence.HasValue
                ? messagesQuery.FirstOrDefault(m => m.Sequence == messageSequence.Value)
                : null;

            // Project to a shape suitable for the frontend
            return Ok(new {
                id = conversation.Id,
                title = conversation.Title,
                createTime = conversation.CreateTime,
                updateTime = conversation.UpdateTime,
                author = conversation.Author,
                isSingleUser = conversation.IsSingleUser,
                gender = conversation.Gender,
                ageCategory = conversation.AgeCategory,
                educationLevel = conversation.EducationLevel,
                currentRegion = conversation.CurrentRegion,
                childhoodRegion = conversation.ChildhoodRegion,
                highlightedMessageId = highlightedMessage?.Id,
                messages = messagesQuery
                    .Select(m => new {
                        id = m.Id,
                        author = m.Author,
                        sequence = m.Sequence,
                        createTime = m.CreateTime,
                        content = m.Content,
                        status = m.Status,
                        endTurn = m.EndTurn,
                        weight = m.Weight,
                        metadata = m.Metadata,
                        recipient = m.Recipient
                    })
                    .ToList()
            });
        }
    }
} 
