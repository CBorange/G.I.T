using GIT_Backend.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GIT_Backend.Infra.Database.Configures;

public class SourceProviderConfigure : IEntityTypeConfiguration<SourceProvider>
{
    public void Configure(EntityTypeBuilder<SourceProvider> entity)
    {
        entity.ToTable("source_provider");

        entity.HasKey(e => e.Id);

        entity.HasIndex(e => e.Code)
            .IsUnique();

        entity.Property(e => e.Id)
            .UseIdentityAlwaysColumn();

        entity.Property(e => e.ExpectCategoryId)
            .IsRequired();

        entity.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();

        entity.Property(e => e.BaseUrl)
            .HasColumnType("text")
            .IsRequired();

        entity.Property(e => e.CrawlUrl)
            .HasColumnType("text")
            .IsRequired();

        entity.Property(e => e.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        entity.Property(e => e.IntervalMin)
            .IsRequired();

        entity.Property(e => e.RateLimitMs)
            .IsRequired();

        entity.Property(e => e.Description)
            .HasMaxLength(200);

        entity.Property(e => e.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        entity.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp with time zone");

        entity.HasOne(e => e.ExpectCategory)
            .WithMany(e => e.SourceProviders)
            .HasForeignKey(e => e.ExpectCategoryId)
            .HasConstraintName("FK_source_category_TO_source_provider")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
