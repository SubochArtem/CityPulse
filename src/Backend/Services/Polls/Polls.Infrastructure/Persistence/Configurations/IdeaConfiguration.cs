using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Polls.Domain.Ideas;
using Polls.Domain.Polls;

namespace Polls.Infrastructure.Persistence.Configurations;

public class IdeaConfiguration : IEntityTypeConfiguration<Idea>
{
    private const string IdColumnName = "id";
    private const string TitleColumnName = "title";
    private const string DescriptionColumnName = "description";
    private const string StatusColumnName = "status";
    private const string CreatedAtColumnName = "created_at";
    private const string UpdatedAtColumnName = "updated_at";
    private const string PollIdIndexName = "ix_idea_poll_id";
    private const string UserIdColumnName = "user_id";
    private const string PollIdColumnName = "poll_id";
    private const int TitleMaxLength = 200;
    private const int DescriptionMaxLength = 1000;

    public void Configure(EntityTypeBuilder<Idea> builder)
    {
        builder.HasKey(i => i.Id);
        
        builder.Property(i => i.Id)
            .HasColumnName(IdColumnName);
        
        builder.Property(i => i.UserId)
            .HasColumnName(UserIdColumnName)
            .IsRequired();
        
        builder.Property(i => i.PollId)
            .HasColumnName(PollIdColumnName)
            .IsRequired();

        builder.Property(i => i.Title)
            .HasColumnName(TitleColumnName)
            .IsRequired()
            .HasMaxLength(TitleMaxLength);

        builder.Property(i => i.Description)
            .HasColumnName(DescriptionColumnName)
            .HasMaxLength(DescriptionMaxLength);

        builder.Property(i => i.Status)
            .HasColumnName(StatusColumnName)
            .IsRequired();

        builder.Property(i => i.CreatedAt)
            .HasColumnName(CreatedAtColumnName)
            .IsRequired();

        builder.Property(i => i.UpdatedAt)
            .HasColumnName(UpdatedAtColumnName)
            .IsRequired();

        builder.HasOne<Poll>()
            .WithMany()
            .HasForeignKey(i => i.PollId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => i.PollId)
            .HasDatabaseName(PollIdIndexName);
    }
}
