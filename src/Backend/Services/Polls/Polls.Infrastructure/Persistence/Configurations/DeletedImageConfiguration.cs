using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Polls.Domain.Images;

namespace Polls.Infrastructure.Persistence.Configurations;

public class DeletedImageConfiguration : IEntityTypeConfiguration<DeletedImage>
{
    private const string TableName = "deleted_images";
    private const string IdColumnName = "id";
    private const string FileNameColumnName = "file_name";
    private const string QueuedAtColumnName = "queued_at";
    private const int FileNameMaxLength = 500;

    public void Configure(EntityTypeBuilder<DeletedImage> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName(IdColumnName);

        builder.Property(x => x.FileName)
            .HasColumnName(FileNameColumnName)
            .IsRequired()
            .HasMaxLength(FileNameMaxLength);

        builder.Property(x => x.QueuedAt)
            .HasColumnName(QueuedAtColumnName)
            .IsRequired();
    }
}
