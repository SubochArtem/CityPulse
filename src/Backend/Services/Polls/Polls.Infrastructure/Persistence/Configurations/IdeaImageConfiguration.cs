using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Polls.Domain.Ideas;
using Polls.Domain.Images;

namespace Polls.Infrastructure.Persistence.Configurations;

public class IdeaImageConfiguration : IEntityTypeConfiguration<IdeaImage>
{
    private const string TableName = "idea_images";
    private const string IdColumnName = "id";
    private const string FileNameColumnName = "file_name";
    private const string OrderColumnName = "order";
    private const string IdeaIdColumnName = "idea_id";
    private const string IdeaIdIndexName = "ix_idea_images_idea_id";
    private const int FileNameMaxLength = 500;

    public void Configure(EntityTypeBuilder<IdeaImage> builder)
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

        builder.Property(x => x.IdeaId)
            .HasColumnName(IdeaIdColumnName)
            .IsRequired();

        builder.HasOne<Idea>()
            .WithMany(i => i.Images)
            .HasForeignKey(x => x.IdeaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.IdeaId)
            .HasDatabaseName(IdeaIdIndexName);
    }
}
