using System.Text;
using GIT_Backend.Domain.Entity;
using GIT_Backend.Infra.Database.Configures;
using Microsoft.EntityFrameworkCore;

namespace GIT_Backend.Infra.Database;

public class GITDBContext(DbContextOptions<GITDBContext> options) : DbContext(options)
{
    public DbSet<AnalysisRoute> AnalysisRoutes => Set<AnalysisRoute>();

    public DbSet<AnalyzedContent> AnalyzedContents => Set<AnalyzedContent>();

    public DbSet<AnalyzerProvider> AnalyzerProviders => Set<AnalyzerProvider>();

    public DbSet<RawContent> RawContents => Set<RawContent>();

    public DbSet<SourceCategory> SourceCategories => Set<SourceCategory>();

    public DbSet<SourceProvider> SourceProviders => Set<SourceProvider>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new AnalysisRouteConfigure());
        modelBuilder.ApplyConfiguration(new AnalyzedContentConfigure());
        modelBuilder.ApplyConfiguration(new AnalyzerProviderConfigure());
        modelBuilder.ApplyConfiguration(new RawContentConfigure());
        modelBuilder.ApplyConfiguration(new SourceCategoryConfigure());
        modelBuilder.ApplyConfiguration(new SourceProviderConfigure());
    }
}
