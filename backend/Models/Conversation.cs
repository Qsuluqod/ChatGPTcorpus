using System;
using System.Collections.Generic;

namespace ChatGPTcorpus.Models
{
    public class Conversation
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public Dictionary<string, Message> Messages { get; set; } = new Dictionary<string, Message>();
        public string Author { get; set; } = string.Empty;
        public string ImportBatchId { get; set; } = string.Empty;
        public bool IsSingleUser { get; set; } = false;
        public string Gender { get; set; } = string.Empty;
        public string AgeCategory { get; set; } = string.Empty;
        public string EducationLevel { get; set; } = string.Empty;
        public string CurrentRegion { get; set; } = string.Empty;
        public string ChildhoodRegion { get; set; } = string.Empty;
    }

    public class MessageNode
    {
        public string Id { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Parent { get; set; } = string.Empty;
        public List<string> Children { get; set; } = new List<string>();
    }

    public class Message
    {
        public string Id { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime CreateTime { get; set; }
        public int Sequence { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public object? EndTurn { get; set; } = null;
        public int Weight { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        public string Recipient { get; set; } = string.Empty;
    }

    public class Author
    {
        public string Role { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
} 
