using GIT_Backend.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace GIT_Backend.Infra;

public class DBContext(DbContextOptions<DBContext> options) : DbContext(options)
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

        ConfigureAnalysisRoute(modelBuilder);
        ConfigureAnalyzedContent(modelBuilder);
        ConfigureAnalyzerProvider(modelBuilder);
        ConfigureRawContent(modelBuilder);
        ConfigureSourceCategory(modelBuilder);
        ConfigureSourceProvider(modelBuilder);
    }

    private static void ConfigureAnalysisRoute(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnalysisRoute>(entity =>
        {
            entity.ToTable("analysis_route");

            entity.HasKey(e => e.Id);

            entity.HasIndex(e => new { e.SourceProviderId, e.AnalyzerProviderId })
                .IsUnique()
                .HasDatabaseName("UQ_analysis_route_source_analyzer");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.SourceProviderId)
                .HasColumnName("source_provider_id")
                .IsRequired();

            entity.Property(e => e.AnalyzerProviderId)
                .HasColumnName("analyzer_provider_id")
                .IsRequired();

            entity.Property(e => e.IsEnabled)
                .HasColumnName("is_enabled")
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
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
        });
    }

    private static void ConfigureAnalyzedContent(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnalyzedContent>(entity =>
        {
            entity.ToTable("analyzed_contents");

            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.RawContentId)
                .IsUnique();

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            entity.Property(e => e.RawContentId)
                .HasColumnName("raw_content_id")
                .IsRequired();

            entity.Property(e => e.AnalyzerProviderId)
                .HasColumnName("analyzer_provider_id")
                .IsRequired();

            entity.Property(e => e.ActualCategoryId)
                .HasColumnName("actual_category_id")
                .IsRequired();

            entity.Property(e => e.TitleSummary)
                .HasColumnName("title_summary")
                .HasColumnType("text")
                .IsRequired();

            entity.Property(e => e.BodySummary)
                .HasColumnName("body_summary")
                .HasColumnType("text")
                .IsRequired();

            entity.Property(e => e.KeywordJson)
                .HasColumnName("keyword_json")
                .HasColumnType("jsonb");

            entity.Property(e => e.LocationJson)
                .HasColumnName("location_json")
                .HasColumnType("jsonb");

            entity.Property(e => e.ModelName)
                .HasColumnName("model_name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.AnalysisPayloadJson)
                .HasColumnName("analysis_payload_json")
                .HasColumnType("jsonb");

            entity.Property(e => e.AnalyzedAt)
                .HasColumnName("analyzed_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

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
        });
    }

    private static void ConfigureAnalyzerProvider(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnalyzerProvider>(entity =>
        {
            entity.ToTable("analyzer_provider");

            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.Code)
                .IsUnique();

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Code)
                .HasColumnName("code")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.ProviderType)
                .HasColumnName("provider_type")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.ModelName)
                .HasColumnName("model_name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.EndpointUrl)
                .HasColumnName("endpoint_url")
                .HasColumnType("text");

            entity.Property(e => e.IsEnabled)
                .HasColumnName("is_enabled")
                .IsRequired();

            entity.Property(e => e.ConfigJson)
                .HasColumnName("config_json")
                .HasColumnType("jsonb");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp with time zone");
        });
    }

    private static void ConfigureRawContent(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RawContent>(entity =>
        {
            entity.ToTable("raw_contents");

            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.SourceUrl)
                .IsUnique();

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            entity.Property(e => e.SourceProviderId)
                .HasColumnName("source_provider_id")
                .IsRequired();

            entity.Property(e => e.ExpectCategoryId)
                .HasColumnName("expect_category_id")
                .IsRequired();

            entity.Property(e => e.SourceUrl)
                .HasColumnName("source_url")
                .HasColumnType("text")
                .IsRequired();

            entity.Property(e => e.Author)
                .HasColumnName("author")
                .HasMaxLength(100);

            entity.Property(e => e.PublishedAt)
                .HasColumnName("published_at")
                .HasColumnType("timestamp with time zone");

            entity.Property(e => e.Title)
                .HasColumnName("title")
                .HasColumnType("text")
                .IsRequired();

            entity.Property(e => e.Body)
                .HasColumnName("body")
                .HasColumnType("text");

            entity.Property(e => e.RawPayloadJson)
                .HasColumnName("raw_payload_json")
                .HasColumnType("jsonb");

            entity.Property(e => e.CrawledAt)
                .HasColumnName("crawled_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
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
        });
    }

    private static void ConfigureSourceCategory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SourceCategory>(entity =>
        {
            entity.ToTable("source_category");

            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.Code)
                .IsUnique();

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.Code)
                .HasColumnName("code")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(200);

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp with time zone");
        });
    }

    private static void ConfigureSourceProvider(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SourceProvider>(entity =>
        {
            entity.ToTable("source_provider");

            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.Code)
                .IsUnique();

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.ExpectCategoryId)
                .HasColumnName("expect_category_id")
                .IsRequired();

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Code)
                .HasColumnName("code")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.BaseUrl)
                .HasColumnName("base_url")
                .HasColumnType("text")
                .IsRequired();

            entity.Property(e => e.CrawlUrl)
                .HasColumnName("crawl_url")
                .HasColumnType("text")
                .IsRequired();

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true)
                .IsRequired();

            entity.Property(e => e.IntervalMin)
                .HasColumnName("interval_min")
                .IsRequired();

            entity.Property(e => e.RateLimitMs)
                .HasColumnName("rate_limit_ms")
                .IsRequired();

            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(200);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp with time zone");

            entity.HasOne(e => e.ExpectCategory)
                .WithMany(e => e.SourceProviders)
                .HasForeignKey(e => e.ExpectCategoryId)
                .HasConstraintName("FK_source_category_TO_source_provider")
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
