using GIT_Backend.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GIT_Backend.Infra.Database.Configures;

public class CrawlTargetConfigure : IEntityTypeConfiguration<CrawlTarget>
{
    public void Configure(EntityTypeBuilder<CrawlTarget> entity)
    {
        entity.ToTable("crawl_target");

        entity.HasKey(e => e.Id);

        entity.HasIndex(e => e.Code)
            .IsUnique();

        entity.Property(e => e.Id)
            .UseIdentityAlwaysColumn();

        entity.Property(e => e.SourceProviderId)
            .IsRequired();

        entity.Property(e => e.SourceCategoryId)
            .IsRequired();

        entity.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();

        entity.Property(e => e.EntryUrl)
            .HasColumnType("text")
            .IsRequired();

        entity.Property(e => e.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        entity.Property(e => e.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("now()");

        entity.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp with time zone");

        entity.Property(e => e.LastRunningAt)
            .HasColumnType("timestamp with time zone");

        entity.HasOne(e => e.SourceProvider)
            .WithMany(e => e.CrawlTargets)
            .HasForeignKey(e => e.SourceProviderId)
            .HasConstraintName("FK_source_provider_TO_crawl_target")
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(e => e.SourceCategory)
            .WithMany(e => e.CrawlTargets)
            .HasForeignKey(e => e.SourceCategoryId)
            .HasConstraintName("FK_source_category_TO_crawl_target")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
