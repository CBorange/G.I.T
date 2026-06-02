using GIT_Backend.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GIT_Backend.Infra.Database.Configures;

public class AnalyzedContentConfigure : IEntityTypeConfiguration<AnalyzedContent>
{
    public void Configure(EntityTypeBuilder<AnalyzedContent> entity)
    {
        entity.ToTable("analyzed_contents", table =>
        {
            table.HasCheckConstraint(
                "CK_analyzed_contents_confidence_range",
                "confidence >= 0 AND confidence <= 1");
        });

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Id)
            .ValueGeneratedNever();

        entity.Property(e => e.RawContentId)
            .IsRequired();

        entity.Property(e => e.AnalyzerProviderId)
            .IsRequired();

        entity.Property(e => e.AnalyzeJobId)
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

        entity.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp with time zone");

        entity.Property(e => e.Confidence)
            .HasColumnType("numeric(5,4)")
            .IsRequired();

        entity.Property(e => e.ConfidenceReason)
            .HasColumnType("text")
            .IsRequired();

        entity.HasOne(e => e.RawContent)
            .WithOne(e => e.AnalyzedContent)
            .HasForeignKey<AnalyzedContent>(e => e.RawContentId)
            .HasConstraintName("FK_raw_contents_TO_analyzed_contents")
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(e => e.AnalyzeJob)
            .WithOne(e => e.AnalyzedContent)
            .HasForeignKey<AnalyzedContent>(e => e.AnalyzeJobId)
            .HasConstraintName("FK_ analyze_jobs_TO_analyzed_contents")
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
