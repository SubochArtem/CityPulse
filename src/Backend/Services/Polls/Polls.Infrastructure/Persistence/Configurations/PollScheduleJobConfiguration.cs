using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Polls.Domain.Polls;
using Polls.Domain.PollScheduleJob;

namespace Polls.Infrastructure.Persistence.Configurations;

public class PollScheduleJobConfiguration : IEntityTypeConfiguration<PollScheduleJob>
{
    private const string TableName = "poll_schedule_jobs";
    private const string IdColumnName = "id";
    private const string PollIdColumnName = "poll_id";
    private const string HangfireJobIdColumnName = "hangfire_job_id";
    private const string PollIdIndexName = "ix_poll_schedule_job_poll_id";
    
    private const int HangfireJobIdMaxLength = 128;

    public void Configure(EntityTypeBuilder<PollScheduleJob> builder)
    {
        builder.ToTable(TableName);
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasColumnName(IdColumnName);
        
        builder.Property(x => x.PollId)
            .HasColumnName(PollIdColumnName)
            .IsRequired();

        builder.Property(x => x.HangfireJobId)
            .HasColumnName(HangfireJobIdColumnName)
            .IsRequired()
            .HasMaxLength(HangfireJobIdMaxLength);

        builder.HasIndex(x => x.PollId)
            .HasDatabaseName(PollIdIndexName)
            .IsUnique();
        
        builder.HasOne<Poll>()
            .WithOne()
            .HasForeignKey<PollScheduleJob>(x => x.PollId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
