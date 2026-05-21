using GIT_Backend.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GIT_Backend.Infra.Database.Configures;

public class AnalysisRouteConfigure : IEntityTypeConfiguration<AnalysisRoute>
{
    public void Configure(EntityTypeBuilder<AnalysisRoute> entity)
    {
        entity.ToTable("analysis_route");

        entity.HasKey(e => e.Id);

        entity.HasIndex(e => new { e.SourceProviderId, e.AnalyzerProviderId })
            .IsUnique()
            .HasDatabaseName("UQ_analysis_route_source_analyzer");

        entity.Property(e => e.Id)
            .UseIdentityAlwaysColumn();

        entity.Property(e => e.SourceProviderId)
            .IsRequired();

        entity.Property(e => e.AnalyzerProviderId)
            .IsRequired();

        entity.Property(e => e.IsEnabled)
            .IsRequired();

        entity.Property(e => e.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        entity.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp with time zone");

        entity.HasOne(e => e.SourceProvider)
            .WithMany(e => e.AnalysisRoutes)
            .HasForeignKey(e => e.SourceProviderId)
            .HasConstraintName("FK_source_provider_TO_analysis_route")
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(e => e.AnalyzerProvider)
            .WithMany(e => e.AnalysisRoutes)
            .HasForeignKey(e => e.AnalyzerProviderId)
            .HasConstraintName("FK_analyzer_provider_TO_analysis_route")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
