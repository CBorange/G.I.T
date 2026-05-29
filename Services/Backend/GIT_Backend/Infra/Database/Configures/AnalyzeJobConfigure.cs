using GIT_Backend.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GIT_Backend.Infra.Database.Configures;

public class AnalyzeJobConfigure : IEntityTypeConfiguration<AnalyzeJob>
{
    public void Configure(EntityTypeBuilder<AnalyzeJob> entity)
    {
        entity.ToTable(" analyze_jobs");

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Id)
            .ValueGeneratedNever();

        entity.Property(e => e.RawContentId)
            .HasColumnName("raw_contents_id")
            .IsRequired();

        entity.Property(e => e.AnalyzerProviderId)
            .IsRequired();

        entity.Property(e => e.PromptPolicyCode)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(e => e.Status)
            .HasMaxLength(20)
            .IsRequired();

        entity.Property(e => e.AttemptCount)
            .IsRequired();

        entity.Property(e => e.MaxAttemptCount)
            .HasColumnName("max_atempt_count");

        entity.Property(e => e.LastError)
            .HasColumnType("text");

        entity.Property(e => e.LastRunningAt)
            .HasColumnType("timestamp with time zone");

        entity.Property(e => e.EndedAt)
            .HasColumnType("timestamp with time zone");

        entity.HasOne(e => e.RawContent)
            .WithOne(e => e.AnalyzeJob)
            .HasForeignKey<AnalyzeJob>(e => e.RawContentId)
            .HasConstraintName("FK_raw_contents_TO_ analyze_jobs")
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(e => e.AnalyzerProvider)
            .WithMany(e => e.AnalyzeJobs)
            .HasForeignKey(e => e.AnalyzerProviderId)
            .HasConstraintName("FK_analyzer_provider_TO_ analyze_jobs")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
