using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Polls.Domain.Cities;
using Polls.Domain.Polls;

namespace Polls.Infrastructure.Persistence.Configurations;

public class PollConfiguration : IEntityTypeConfiguration<Poll>
{
    public void Configure(EntityTypeBuilder<Poll> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(u => u.Id)
            .HasColumnName("id");

        builder.Property(p => p.BudgetAmount)
            .HasColumnName("budget_amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.Type)
            .HasColumnName("type")
            .IsRequired();

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(p => p.EndsAt)
            .HasColumnName("ends_at")
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasOne<City>()
            .WithMany()
            .HasForeignKey(p => p.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.CityId)
            .HasDatabaseName("ix_poll_city_id");
    }
}
