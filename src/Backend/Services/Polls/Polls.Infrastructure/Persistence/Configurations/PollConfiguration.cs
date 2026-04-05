using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Polls.Domain.Cities;
using Polls.Domain.Polls;

namespace Polls.Infrastructure.Persistence.Configurations;

public class PollConfiguration : IEntityTypeConfiguration<Poll>
{
    private const string IdColumnName = "id";
    private const string BudgetAmountColumnName = "budget_amount";
    private const string BudgetAmountColumnType = "decimal(18,2)";
    private const string TypeColumnName = "type";
    private const string StatusColumnName = "status";
    private const string EndsAtColumnName = "ends_at";
    private const string CreatedAtColumnName = "created_at";
    private const string UpdatedAtColumnName = "updated_at";
    private const string CityIdIndexName = "ix_poll_city_id";
    private const string TitleColumnName = "title";
    private const string DescriptionColumnName = "description";
    private const string CityIdColumnName = "city_id";
    
    private const int TitleMaxLength = 200;
    private const int DescriptionMaxLength = 1000;


    public void Configure(EntityTypeBuilder<Poll> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Id)
            .HasColumnName(IdColumnName);
        
        builder.Property(p => p.CityId)
            .HasColumnName(CityIdColumnName)
            .IsRequired();
        
        builder.Property(p => p.Title)
            .HasColumnName(TitleColumnName)
            .IsRequired()
            .HasMaxLength(TitleMaxLength);

        builder.Property(p => p.Description)
            .HasColumnName(DescriptionColumnName)
            .HasMaxLength(DescriptionMaxLength);

        builder.Property(p => p.BudgetAmount)
            .HasColumnName(BudgetAmountColumnName)
            .HasColumnType(BudgetAmountColumnType)
            .IsRequired();

        builder.Property(p => p.Type)
            .HasColumnName(TypeColumnName)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasColumnName(StatusColumnName)
            .IsRequired();

        builder.Property(p => p.EndsAt)
            .HasColumnName(EndsAtColumnName)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName(CreatedAtColumnName)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName(UpdatedAtColumnName)
            .IsRequired();

        builder.HasOne<City>()
            .WithMany()
            .HasForeignKey(p => p.CityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.CityId)
            .HasDatabaseName(CityIdIndexName);
    }
}
