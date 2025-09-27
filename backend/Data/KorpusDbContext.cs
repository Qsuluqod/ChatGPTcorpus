using Microsoft.EntityFrameworkCore;
using ChatGPTcorpus.Models;

namespace ChatGPTcorpus.Data
{
    public class KorpusDbContext : DbContext
    {
        public KorpusDbContext(DbContextOptions<KorpusDbContext> options) : base(options)
        {
        }

        public DbSet<DbConversation> Conversations { get; set; }
        public DbSet<DbMessage> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DbConversation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.Author).IsRequired();
                entity.Property(e => e.ImportBatchId).IsRequired();
                entity.Property(e => e.CreateTime).IsRequired();
                entity.Property(e => e.UpdateTime).IsRequired();
                
                // Configure the one-to-many relationship with messages
                entity.HasMany(e => e.Messages)
                      .WithOne(e => e.Conversation)
                      .HasForeignKey(e => e.ConversationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<DbMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Author).IsRequired();
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.CreateTime).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                
                // Configure the relationship with conversation
                entity.HasOne(e => e.Conversation)
                      .WithMany(e => e.Messages)
                      .HasForeignKey(e => e.ConversationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
} 
