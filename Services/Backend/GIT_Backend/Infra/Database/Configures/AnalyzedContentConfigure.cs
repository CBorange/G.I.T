using GIT_Backend.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GIT_Backend.Infra.Database.Configures;

public class AnalyzedContentConfigure : IEntityTypeConfiguration<AnalyzedContent>
{
    public void Configure(EntityTypeBuilder<AnalyzedContent> entity)
    {
        entity.ToTable("analyzed_contents");

        entity.HasKey(e => e.Id);

        entity.HasIndex(e => e.RawContentId)
            .IsUnique();

        entity.Property(e => e.Id)
            .ValueGeneratedNever();

        entity.Property(e => e.RawContentId)
            .IsRequired();

        entity.Property(e => e.AnalyzerProviderId)
            .IsRequired();

        entity.Property(e => e.ActualCategoryId)
            .IsRequired();

        entity.Property(e => e.TitleSummary)
            .HasColumnType("text")
            .IsRequired();

        entity.Property(e => e.BodySummary)
            .HasColumnType("text")
            .IsRequired();

        entity.Property(e => e.KeywordJson)
            .HasColumnType("jsonb");

        entity.Property(e => e.LocationJson)
            .HasColumnType("jsonb");

        entity.Property(e => e.ModelName)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(e => e.AnalysisPayloadJson)
            .HasColumnType("jsonb");

        entity.Property(e => e.AnalyzedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        entity.Property(e => e.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("now()");

        entity.HasOne(e => e.RawContent)
            .WithOne(e => e.AnalyzedContent)
            .HasForeignKey<AnalyzedContent>(e => e.RawContentId)
            .HasConstraintName("FK_raw_contents_TO_analyzed_contents")
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(e => e.AnalyzerProvider)
            .WithMany(e => e.AnalyzedContents)
            .HasForeignKey(e => e.AnalyzerProviderId)
            .HasConstraintName("FK_analyzer_provider_TO_analyzed_contents")
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(e => e.ActualCategory)
            .WithMany(e => e.AnalyzedContents)
            .HasForeignKey(e => e.ActualCategoryId)
            .HasConstraintName("FK_source_category_TO_analyzed_contents")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
