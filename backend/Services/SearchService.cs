using System;
using System.Collections.Generic;
using System.Linq;
using ChatGPTcorpus.Models;

namespace ChatGPTcorpus.Services
{
    public class SearchService
    {
        private readonly List<Conversation> _conversations;

        public SearchService(List<Conversation> conversations)
        {
            _conversations = conversations ?? throw new ArgumentNullException(nameof(conversations));
        }

        public IEnumerable<(Conversation Conversation, Message Message)> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException("Search query cannot be empty", nameof(query));
            }

            query = query.ToLowerInvariant();

            return _conversations
                .SelectMany(conversation => conversation.Messages.Values
                    .OrderBy(message => message.Sequence)
                    .Where(message => 
                        message.Content?.ToLowerInvariant().Contains(query) == true ||
                        message.Author?.ToLowerInvariant().Contains(query) == true)
                    .Select(message => (conversation, message)));
        }

        public IEnumerable<Conversation> SearchByTitle(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException("Search query cannot be empty", nameof(query));
            }

            query = query.ToLowerInvariant();

            return _conversations
                .Where(conversation => 
                    conversation.Title?.ToLowerInvariant().Contains(query) == true);
        }

        public IEnumerable<(Conversation Conversation, Message Message)> SearchByAuthor(string author)
        {
            if (string.IsNullOrWhiteSpace(author))
            {
                throw new ArgumentException("Author cannot be empty", nameof(author));
            }

            author = author.ToLowerInvariant();

            return _conversations
                .SelectMany(conversation => conversation.Messages.Values
                    .OrderBy(message => message.Sequence)
                    .Where(message => message.Author?.ToLowerInvariant() == author)
                    .Select(message => (conversation, message)));
        }

        public IEnumerable<(Conversation Conversation, Message Message)> SearchByDateRange(DateTime startDate, DateTime endDate)
        {
            return _conversations
                .SelectMany(conversation => conversation.Messages.Values
                    .OrderBy(message => message.Sequence)
                    .Where(message => message.CreateTime >= startDate && message.CreateTime <= endDate)
                    .Select(message => (conversation, message)));
        }
    }
} 
