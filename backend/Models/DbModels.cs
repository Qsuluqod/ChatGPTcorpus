using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace ChatGPTcorpus.Models
{
    public class DbConversation
    {
        [Key]
        public string Id { get; set; } = string.Empty;
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public DateTime CreateTime { get; set; }
        
        [Required]
        public DateTime UpdateTime { get; set; }
        
        [Required]
        public string Author { get; set; } = string.Empty;

        [Required]
        public string ImportBatchId { get; set; } = string.Empty;

        public bool IsSingleUser { get; set; }
        
        public string? Gender { get; set; }
        
        public string? AgeCategory { get; set; }
        
        public string? EducationLevel { get; set; }
        
        public string? CurrentRegion { get; set; }
        
        public string? ChildhoodRegion { get; set; }

        // Navigation property
        public virtual ICollection<DbMessage> Messages { get; set; } = new List<DbMessage>();
    }

    public class DbMessage
    {
        [Key]
        public string Id { get; set; } = string.Empty;
        
        [Required]
        public string Author { get; set; } = string.Empty;
        
        [Required]
        public DateTime CreateTime { get; set; }
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        [Required]
        public string Status { get; set; } = string.Empty;
        
        public string? EndTurn { get; set; }
        
        public int Weight { get; set; }
        
        // Store metadata as JSON string
        public string MetadataJson { get; set; } = "{}";
        
        public string Recipient { get; set; } = string.Empty;

        // Foreign key
        [Required]
        public string ConversationId { get; set; } = string.Empty;

        // Navigation property
        [ForeignKey("ConversationId")]
        public virtual DbConversation Conversation { get; set; } = null!;

        // Helper properties to convert between Dictionary and JSON
        [NotMapped]
        public Dictionary<string, object> Metadata
        {
            get => JsonSerializer.Deserialize<Dictionary<string, object>>(MetadataJson) ?? new Dictionary<string, object>();
            set => MetadataJson = JsonSerializer.Serialize(value);
        }
    }
} 
