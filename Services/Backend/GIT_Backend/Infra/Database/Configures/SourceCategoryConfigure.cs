using GIT_Backend.Domain.Constants;
using GIT_Backend.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GIT_Backend.Infra.Database.Configures;

public class SourceCategoryConfigure : IEntityTypeConfiguration<SourceCategory>
{
    public void ConfigureSeedData(EntityTypeBuilder<SourceCategory> entity)
    {
        entity.HasData(
            new SourceCategory
            {
                Id = 1,
                Code = SourceCategoryCodes.Culture,
                Name = "문화",
                Description = "문화 관련 이슈"
            },
            new SourceCategory
            {
                Id = 2,
                Code = SourceCategoryCodes.Economy,
                Name = "경제",
                Description = "경제 관련 이슈"
            },
            new SourceCategory
            {
                Id = 3,
                Code = SourceCategoryCodes.Welfare,
                Name = "복지",
                Description = "복지 관련 이슈"
            },
            new SourceCategory
            {
                Id = 4,
                Code = SourceCategoryCodes.Transport,
                Name = "교통",
                Description = "교통 관련 이슈"
            },
            new SourceCategory
            {
                Id = 5,
                Code = SourceCategoryCodes.Environment,
                Name = "환경",
                Description = "환경 관련 이슈"
            },
            new SourceCategory
            {
                Id = 6,
                Code = SourceCategoryCodes.Housing,
                Name = "주택",
                Description = "주택 관련 이슈"
            },
            new SourceCategory
            {
                Id = 7,
                Code = SourceCategoryCodes.Safety,
                Name = "안전",
                Description = "안전 관련 이슈"
            },
            new SourceCategory
            {
                Id = 8,
                Code = SourceCategoryCodes.Administration,
                Name = "행정",
                Description = "행정 관련 이슈"
            }
        );
    }

    public void Configure(EntityTypeBuilder<SourceCategory> entity)
    {
        entity.ToTable("source_category");

        entity.HasKey(e => e.Id);

        entity.HasIndex(e => e.Code)
            .IsUnique();

        entity.Property(e => e.Id)
            .UseIdentityAlwaysColumn();

        entity.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();

        entity.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(e => e.Description)
            .HasMaxLength(200);

        entity.Property(e => e.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("now()");

        entity.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp with time zone");

        ConfigureSeedData(entity);
    }
}
