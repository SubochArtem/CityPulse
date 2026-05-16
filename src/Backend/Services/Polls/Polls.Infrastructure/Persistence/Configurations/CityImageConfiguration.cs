using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Polls.Domain.Cities;
using Polls.Domain.Images;

namespace Polls.Infrastructure.Persistence.Configurations;

public class CityImageConfiguration : IEntityTypeConfiguration<CityImage>
{
    private const string TableName = "city_images";
    private const string IdColumnName = "id";
    private const string FileNameColumnName = "file_name";
    private const string OrderColumnName = "order";
    private const string CityIdColumnName = "city_id";
    private const string CityIdIndexName = "ix_city_images_city_id";
    private const int FileNameMaxLength = 500;

    public void Configure(EntityTypeBuilder<CityImage> builder)
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

        builder.Property(x => x.CityId)
            .HasColumnName(CityIdColumnName)
            .IsRequired();

        builder.HasOne<City>()
            .WithMany(c => c.Images)
            .HasForeignKey(x => x.CityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.CityId)
            .HasDatabaseName(CityIdIndexName);
    }
}
