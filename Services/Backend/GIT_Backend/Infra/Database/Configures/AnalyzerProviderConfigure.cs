using GIT_Backend.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GIT_Backend.Infra.Database.Configures;

public class AnalyzerProviderConfigure : IEntityTypeConfiguration<AnalyzerProvider>
{
    public void Configure(EntityTypeBuilder<AnalyzerProvider> entity)
    {
        entity.ToTable("analyzer_provider");

        entity.HasKey(e => e.Id);

        entity.HasIndex(e => e.Code)
            .IsUnique();

        entity.Property(e => e.Id)
            .UseIdentityAlwaysColumn();

        entity.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();

        entity.Property(e => e.ProviderType)
            .HasMaxLength(50)
            .IsRequired();

        entity.Property(e => e.ModelName)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(e => e.EndpointUrl)
            .HasColumnType("text");

        entity.Property(e => e.IsEnabled)
            .IsRequired();

        entity.Property(e => e.ConfigJson)
            .HasColumnType("jsonb");

        entity.Property(e => e.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("now()");

        entity.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp with time zone");
    }
}
