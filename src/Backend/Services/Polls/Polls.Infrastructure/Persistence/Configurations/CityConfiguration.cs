using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Polls.Domain.Cities;

namespace Polls.Infrastructure.Persistence.Configurations;

public class CityConfiguration : IEntityTypeConfiguration<City>
{
    private const string IdColumnName = "id";
    private const string NameColumnName = "name";
    private const string DescriptionColumnName = "description";
    private const string LatitudeColumnName = "latitude";
    private const string LongitudeColumnName = "longitude";
    private const string StatusColumnName = "status";
    private const string CreatedAtColumnName = "created_at";
    private const string UpdatedAtColumnName = "updated_at";
    
    private const int NameMaxLength = 100;
    private const int DescriptionMaxLength = 500;

    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Id)
            .HasColumnName(IdColumnName);

        builder.Property(c => c.Title)
            .HasColumnName(NameColumnName)
            .IsRequired()
            .HasMaxLength(NameMaxLength);

        builder.Property(c => c.Description)
            .HasColumnName(DescriptionColumnName)
            .HasMaxLength(DescriptionMaxLength);

        builder.OwnsOne(c => c.Coordinates, coords =>
        {
            coords.Property(c => c.Latitude)
                .HasColumnName(LatitudeColumnName)
                .IsRequired();

            coords.Property(c => c.Longitude)
                .HasColumnName(LongitudeColumnName)
                .IsRequired();
        });

        builder.Property(c => c.Status)
            .HasColumnName(StatusColumnName)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName(CreatedAtColumnName)
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName(UpdatedAtColumnName)
            .IsRequired();
    }
}
