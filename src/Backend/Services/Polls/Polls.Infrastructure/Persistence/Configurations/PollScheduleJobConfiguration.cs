using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Polls.Infrastructure.Persistence.Entities;

namespace Polls.Infrastructure.Persistence.Configurations;

public class PollScheduleJobConfiguration : IEntityTypeConfiguration<PollScheduleJob>
{
    public void Configure(EntityTypeBuilder<PollScheduleJob> builder)
    {
        builder.ToTable("poll_schedule_jobs");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.PollId)
            .IsRequired();

        builder.Property(x => x.HangfireJobId)
            .IsRequired()
            .HasMaxLength(128);
        
        builder.HasIndex(x => x.PollId)
            .IsUnique();
    }
}
