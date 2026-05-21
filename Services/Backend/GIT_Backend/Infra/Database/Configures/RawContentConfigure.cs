using GIT_Backend.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GIT_Backend.Infra.Database.Configures;

public class RawContentConfigure : IEntityTypeConfiguration<RawContent>
{
    public void Configure(EntityTypeBuilder<RawContent> entity)
    {
        entity.ToTable("raw_contents");

        entity.HasKey(e => e.Id);

        entity.HasIndex(e => e.SourceUrl)
            .IsUnique();

        entity.Property(e => e.Id)
            .ValueGeneratedNever();

        entity.Property(e => e.SourceProviderId)
            .IsRequired();

        entity.Property(e => e.ExpectCategoryId)
            .IsRequired();

        entity.Property(e => e.SourceUrl)
            .HasColumnType("text")
            .IsRequired();

        entity.Property(e => e.Author)
            .HasMaxLength(100);

        entity.Property(e => e.PublishedAt)
            .HasColumnType("timestamp with time zone");

        entity.Property(e => e.Title)
            .HasColumnType("text")
            .IsRequired();

        entity.Property(e => e.Body)
            .HasColumnType("text");

        entity.Property(e => e.RawPayloadJson)
            .HasColumnType("jsonb");

        entity.Property(e => e.CrawledAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        entity.Property(e => e.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        entity.HasOne(e => e.SourceProvider)
            .WithMany(e => e.RawContents)
            .HasForeignKey(e => e.SourceProviderId)
            .HasConstraintName("FK_source_provider_TO_raw_contents")
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(e => e.ExpectCategory)
            .WithMany(e => e.RawContents)
            .HasForeignKey(e => e.ExpectCategoryId)
            .HasConstraintName("FK_source_category_TO_raw_contents")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
