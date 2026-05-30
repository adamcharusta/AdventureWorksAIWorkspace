using AdventureWorksAIWorkspaceAPI.Domain.Reports;
using AdventureWorksAIWorkspaceAPI.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorksAIWorkspaceAPI.Infrastructure.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Report> Reports => Set<Report>();

    public DbSet<ReportConversation> ReportConversations => Set<ReportConversation>();

    public DbSet<ReportMessage> ReportMessages => Set<ReportMessage>();

    public DbSet<GeneratedSqlQuery> GeneratedSqlQueries => Set<GeneratedSqlQuery>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.TokenHash).IsRequired().HasMaxLength(512);
            entity.HasIndex(t => t.TokenHash).IsUnique();
            entity.HasIndex(t => t.UserId);
            entity.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Report>(entity =>
        {
            entity.HasKey(report => report.Id);
            entity.Property(report => report.UserId).IsRequired().HasMaxLength(450);
            entity.Property(report => report.Title).IsRequired().HasMaxLength(256);
            entity.Property(report => report.OriginalPrompt).IsRequired().HasMaxLength(2000);
            entity.Property(report => report.Summary).HasMaxLength(4000);
            entity.Property(report => report.Conclusions).HasMaxLength(4000);
            entity.Property(report => report.ResultJson);
            entity.Property(report => report.ChartsJson);
            entity.Property(report => report.Status).HasConversion<string>().IsRequired().HasMaxLength(32);
            entity.HasIndex(report => new { report.UserId, report.UpdatedAt });
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(report => report.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(report => report.Conversation)
                .WithOne(conversation => conversation.Report)
                .HasForeignKey<ReportConversation>(conversation => conversation.ReportId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ReportConversation>(entity =>
        {
            entity.HasKey(conversation => conversation.Id);
            entity.Property(conversation => conversation.ReportId).IsRequired().HasMaxLength(450);
            entity.HasIndex(conversation => conversation.ReportId).IsUnique();
        });

        builder.Entity<ReportMessage>(entity =>
        {
            entity.HasKey(message => message.Id);
            entity.Property(message => message.ConversationId).IsRequired().HasMaxLength(450);
            entity.Property(message => message.Role).HasConversion<string>().IsRequired().HasMaxLength(32);
            entity.Property(message => message.Content).IsRequired().HasMaxLength(8000);
            entity.Property(message => message.RelatedSqlQueryId).HasMaxLength(450);
            entity.HasIndex(message => new { message.ConversationId, message.SortOrder }).IsUnique();
            entity.HasOne(message => message.Conversation)
                .WithMany(conversation => conversation.Messages)
                .HasForeignKey(message => message.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(message => message.RelatedSqlQuery)
                .WithMany()
                .HasForeignKey(message => message.RelatedSqlQueryId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<GeneratedSqlQuery>(entity =>
        {
            entity.HasKey(query => query.Id);
            entity.Property(query => query.ReportId).IsRequired().HasMaxLength(450);
            entity.Property(query => query.SourceMessageId).HasMaxLength(450);
            entity.Property(query => query.UserPrompt).IsRequired().HasMaxLength(2000);
            entity.Property(query => query.SqlText).IsRequired();
            entity.Property(query => query.Explanation).HasMaxLength(4000);
            entity.Property(query => query.PresentationTitle).HasMaxLength(256);
            entity.Property(query => query.Summary).HasMaxLength(4000);
            entity.Property(query => query.Conclusions).HasMaxLength(4000);
            entity.Property(query => query.ResultJson);
            entity.Property(query => query.ChartsJson);
            entity.Property(query => query.ValidationStatus).HasConversion<string>().IsRequired().HasMaxLength(32);
            entity.Property(query => query.ValidationMessage).HasMaxLength(2000);
            entity.Property(query => query.ExecutionStatus).HasConversion<string>().IsRequired().HasMaxLength(32);
            entity.Property(query => query.ExecutionMessage).HasMaxLength(2000);
            entity.HasIndex(query => new { query.ReportId, query.CreatedAt });
            entity.HasOne(query => query.Report)
                .WithMany(report => report.GeneratedSqlQueries)
                .HasForeignKey(query => query.ReportId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(query => query.SourceMessage)
                .WithMany()
                .HasForeignKey(query => query.SourceMessageId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
