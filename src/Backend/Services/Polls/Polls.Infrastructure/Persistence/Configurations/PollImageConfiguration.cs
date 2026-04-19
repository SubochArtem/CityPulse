using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Polls.Domain.Images;
using Polls.Domain.Polls;

namespace Polls.Infrastructure.Persistence.Configurations;

public class PollImageConfiguration : IEntityTypeConfiguration<PollImage>
{
    private const string TableName = "poll_images";
    private const string IdColumnName = "id";
    private const string FileNameColumnName = "file_name";
    private const string OrderColumnName = "order";
    private const string PollIdColumnName = "poll_id";
    private const string PollIdIndexName = "ix_poll_images_poll_id";
    private const int FileNameMaxLength = 500;

    public void Configure(EntityTypeBuilder<PollImage> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName(IdColumnName);

        builder.Property(x => x.FileName)
            .HasColumnName(FileNameColumnName)
            .IsRequired()
            .HasMaxLength(FileNameMaxLength);

        builder.Property(x => x.Order)
            .HasColumnName(OrderColumnName)
            .IsRequired();

        builder.Property(x => x.PollId)
            .HasColumnName(PollIdColumnName)
            .IsRequired();

        builder.HasOne<Poll>()
            .WithMany(p => p.Images)
            .HasForeignKey(x => x.PollId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.PollId)
            .HasDatabaseName(PollIdIndexName);
    }
}
