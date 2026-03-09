using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Polls.Domain.Ideas;
using Polls.Domain.Polls;

namespace Polls.Infrastructure.Persistence.Configurations;

public class IdeaConfiguration : IEntityTypeConfiguration<Idea>
{
    public void Configure(EntityTypeBuilder<Idea> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedOnAdd();

        builder.Property(i => i.Title)
            .HasColumnName("title")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(i => i.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(i => i.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasOne<Poll>()
            .WithMany()
            .HasForeignKey(i => i.PollId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.PollId)
            .HasDatabaseName("IX_Idea_PollId");
    }
}
