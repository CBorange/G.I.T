# G.I.T Backend 구조 정리: 1차 목표와 2차 목표

## 0. 전제

G.I.T의 현재 개발 목표는 **작동하는 시스템 완성**이다.

따라서 1차 목표에서는 Clean Architecture를 강하게 적용하지 않고, 익숙한 **Layered Architecture**를 기반으로 개발한다. 

다만 장기적으로 구조 개선이 가능하도록 최소한의 의존성 원칙은 지킨다.

---

## 1. 전체 방향

| 구분 | 1차 목표 | 2차 목표 |
|---|---|---|
| 핵심 목표 | 기능 구현, 전체 파이프라인 완성 | 구조 개선, 유지보수성 강화 |
| ASP.NET 구조 | 단일 프로젝트 | 다중 프로젝트 분리 |
| API / Worker | 같은 프로세스에서 실행 | 별도 진입점 프로젝트로 분리 |
| Layer 분리 | 폴더/네임스페이스 단위 | csproj 단위 물리 분리 |
| Domain / EF Entity | 동일 객체 사용 | Domain Model과 EF Entity 분리 |
| Redis Consumer | BackgroundService로 내장 | Worker 프로젝트로 독립 |
| 우선순위 | 돌아가는 시스템 | 명확한 책임 분리 |

---

# Part 1. 1차 목표 구조

## 1.1 1차 목표의 기준

1차 목표는 다음 흐름을 실제로 동작시키는 것이다.

```txt
Python Crawler
→ Redis Stream
→ Python Analyzer
→ Redis Stream
→ ASP.NET Core BackgroundService
→ PostgreSQL
→ ASP.NET Core API
→ React + Leaflet
```

이 단계에서는 다음을 우선한다.

```txt
기능 구현 > 구조적 완벽함
단순성 > 계층 과분리
실행 가능한 시스템 > 이론적으로 순수한 구조
```

---

## 1.2 1차 추천 구조

1차에서는 **단일 ASP.NET Core 프로젝트** 안에서 폴더/네임스페이스로 Layer를 구분한다.

```txt
GIT.Backend/
├─ Controllers/
│  ├─ ArticlesController.cs
│  ├─ CommentsController.cs
│  └─ DistrictsController.cs
│
├─ Application/
│  ├─ Services/
│  │  ├─ ArticleService.cs
│  │  ├─ CommentService.cs
│  │  └─ AnalysisIngestionService.cs
│  │
│  ├─ DTOs/
│  │  ├─ Requests/
│  │  └─ Responses/
│  │
│  ├─ Interfaces/
│  │  ├─ IArticleRepository.cs
│  │  ├─ ICommentRepository.cs
│  │  ├─ IAnalysisRepository.cs
│  │  └─ IUnitOfWork.cs
│  │
│  └─ Mappings/
│
├─ Domain/
│  ├─ Entities/
│  │  ├─ SourceProvider.cs
│  │  ├─ SourceCategory.cs
│  │  ├─ RawContent.cs
│  │  ├─ AnalysisContent.cs
│  │  ├─ AnalysisLocation.cs
│  │  ├─ User.cs
│  │  └─ Comment.cs
│  │
│  ├─ Enums/
│  └─ ValueObjects/
│
├─ Infrastructure/
│  ├─ Persistence/
│  │  ├─ GitDbContext.cs
│  │  ├─ Configurations/
│  │  │  ├─ RawContentConfiguration.cs
│  │  │  ├─ AnalysisContentConfiguration.cs
│  │  │  └─ AnalysisLocationConfiguration.cs
│  │  │
│  │  └─ Repositories/
│  │     ├─ ArticleRepository.cs
│  │     ├─ CommentRepository.cs
│  │     └─ AnalysisRepository.cs
│  │
│  ├─ Redis/
│  │  ├─ RedisStreamConsumer.cs
│  │  ├─ RedisStreamMessageParser.cs
│  │  └─ RedisStreamOptions.cs
│  │
│  └─ External/
│
├─ Workers/
│  ├─ RedisAnalysisResultWorker.cs
│  └─ PendingMessageReprocessor.cs
│
├─ Middlewares/
├─ Filters/
├─ Shared/
│  ├─ Constants/
│  ├─ Exceptions/
│  └─ Extensions/
│
├─ Program.cs
├─ appsettings.json
├─ appsettings.Development.json
└─ GIT.Backend.csproj
```

---

## 1.3 1차 구조의 핵심 원칙

### 원칙 1. 물리 프로젝트 분리는 하지 않는다

1차에서는 다음처럼 프로젝트를 쪼개지 않는다.

```txt
GIT.Backend.Api.csproj
GIT.Backend.Application.csproj
GIT.Backend.Domain.csproj
GIT.Backend.Infrastructure.csproj
GIT.Backend.Worker.csproj
```

대신 하나의 프로젝트 안에서 폴더로만 분리한다.

```txt
GIT.Backend.csproj
├─ Controllers
├─ Application
├─ Domain
├─ Infrastructure
└─ Workers
```

이유는 다음과 같다.

```txt
1. 1차 목표는 기능 구현이 우선이다.
2. 프로젝트 참조 관리로 인한 피로도를 줄인다.
3. DI, 설정, 배포, Docker 구성을 단순하게 유지한다.
4. 나중에 구조 개선 시 폴더 단위로 프로젝트 분리하기 쉽다.
```

---

### 원칙 2. Worker는 API 프로세스 안에서 BackgroundService로 실행한다

1차에서는 Redis Stream Consumer를 별도 Worker 프로젝트로 분리하지 않는다.

```txt
ASP.NET Core Process
├─ HTTP API 요청 처리
└─ BackgroundService
   └─ Redis Stream Consumer Loop
```

등록 예시는 다음과 같다.

```csharp
builder.Services.AddHostedService<RedisAnalysisResultWorker>();
```

이 구조는 다음 상황에 적합하다.

```txt
1. 1차 목표에서 트래픽이 크지 않다.
2. API와 Worker를 따로 스케일링할 필요가 아직 없다.
3. Docker Compose 구성을 단순하게 유지할 수 있다.
4. Redis → DB 저장 흐름을 빠르게 구현할 수 있다.
```

---

### 원칙 3. Domain Entity를 EF Core 매핑 대상으로 같이 사용한다

1차에서는 `Domain/Entities`에 있는 객체를 EF Core Entity로도 사용한다.

```txt
Domain Entity
= 비즈니스 모델
= EF Core CodeFirst 매핑 대상
```

예시:

```csharp
namespace GIT.Backend.Domain.Entities;

public sealed class RawContent
{
    public long Id { get; private set; }
    public string ArticleUrl { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public DateTime PublishedAt { get; private set; }

    private RawContent()
    {
        // EF Core constructor
    }

    public RawContent(string articleUrl, string title, DateTime publishedAt)
    {
        if (string.IsNullOrWhiteSpace(articleUrl))
            throw new ArgumentException("ArticleUrl is required.", nameof(articleUrl));

        ArticleUrl = articleUrl;
        Title = title;
        PublishedAt = publishedAt;
    }
}
```

단, Entity 안에 EF Core Attribute를 최대한 넣지 않는다.

피해야 할 예:

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

[Table("raw_contents")]
[Index(nameof(ArticleUrl), IsUnique = true)]
public class RawContent
{
    [Key]
    public long Id { get; set; }
}
```

추천 방식:

```txt
Entity 자체는 최대한 순수하게 둔다.
DB 매핑 설정은 Infrastructure/Persistence/Configurations 하위의 Fluent API로 처리한다.
```

---

### 원칙 4. Fluent API는 Infrastructure에 둔다

```txt
Infrastructure/Persistence/Configurations/
├─ RawContentConfiguration.cs
├─ AnalysisContentConfiguration.cs
└─ AnalysisLocationConfiguration.cs
```

예시:

```csharp
public sealed class RawContentConfiguration : IEntityTypeConfiguration<RawContent>
{
    public void Configure(EntityTypeBuilder<RawContent> builder)
    {
        builder.ToTable("raw_contents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ArticleUrl)
            .HasColumnName("article_url")
            .HasMaxLength(1000)
            .IsRequired();

        builder.HasIndex(x => x.ArticleUrl)
            .IsUnique();
    }
}
```

이렇게 하면 `Domain`은 EF Core를 직접 참조하지 않고, EF 관련 설정은 `Infrastructure` 쪽에 모인다.

---

### 원칙 5. Controller에서 DbContext를 직접 쓰지 않는다

피해야 할 구조:

```txt
Controller → DbContext
```

추천 구조:

```txt
Controller
→ Application Service
→ Repository Interface
→ Repository Implementation
→ GitDbContext
```

예시:

```txt
ArticlesController
→ ArticleService
→ IArticleRepository
→ ArticleRepository
→ GitDbContext
```

---

### 원칙 6. 조회는 NoTracking, 수정은 Tracking으로 분리한다

Repository 메서드는 의도를 명확히 나눈다.

```csharp
public interface IArticleRepository
{
    Task<RawContent?> GetByIdAsync(long id, CancellationToken ct);          // 조회용, NoTracking
    Task<RawContent?> GetForUpdateAsync(long id, CancellationToken ct);    // 수정용, Tracking
    Task<bool> ExistsByUrlAsync(string articleUrl, CancellationToken ct);
    Task AddAsync(RawContent content, CancellationToken ct);
}
```

조회용:

```csharp
public Task<RawContent?> GetByIdAsync(long id, CancellationToken ct)
{
    return _db.RawContents
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.Id == id, ct);
}
```

수정용:

```csharp
public Task<RawContent?> GetForUpdateAsync(long id, CancellationToken ct)
{
    return _db.RawContents
        .FirstOrDefaultAsync(x => x.Id == id, ct);
}
```

수정 흐름:

```csharp
var article = await _articleRepository.GetForUpdateAsync(id, ct);

if (article is null)
    throw new NotFoundException();

article.UpdateTitle(request.Title);

await _unitOfWork.SaveChangesAsync(ct);
```

여기서 `IUnitOfWork`는 사실상 `DbContext.SaveChangesAsync()`의 래퍼다.

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}
```

```csharp
public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly GitDbContext _db;

    public EfUnitOfWork(GitDbContext db)
    {
        _db = db;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct)
    {
        return _db.SaveChangesAsync(ct);
    }
}
```

1차에서는 `IUnitOfWork`를 두어도 되고, 단순히 `GitDbContext.SaveChangesAsync()`를 직접 호출해도 된다. 다만 Application Layer가 `DbContext`를 직접 알지 않게 하려면 `IUnitOfWork`를 두는 편이 낫다.

---

## 1.4 1차에서 하지 않을 것

1차에서는 다음을 억지로 하지 않는다.

```txt
1. Application/Domain/Infrastructure를 csproj 단위로 분리하지 않는다.
2. API와 Worker를 별도 실행 프로젝트로 분리하지 않는다.
3. Domain Model과 EF Entity를 완전히 분리하지 않는다.
4. CQRS, MediatR, UseCase 클래스를 무리하게 도입하지 않는다.
5. Outbox Pattern, Domain Event, 강한 Transaction Script 구조를 처음부터 넣지 않는다.
```

---

# Part 2. 2차 목표 구조

## 2.1 2차 목표의 기준

2차 목표는 1차에서 완성한 시스템을 기반으로 구조를 개선하는 단계다.

이 단계에서는 다음을 검토한다.

```txt
1. API와 Worker 실행 단위 분리
2. Application/Domain/Infrastructure 프로젝트 분리
3. Domain Model과 EF Entity 분리
4. Redis 메시지 처리 구조 정리
5. 테스트 가능한 구조로 개선
6. 트랜잭션/재처리/장애 복구 정책 강화
```

---

## 2.2 2차 추천 구조

2차에서는 다음처럼 프로젝트 단위로 분리할 수 있다.

```txt
GIT.Backend/
├─ GIT.Backend.Api/
│  ├─ Controllers/
│  ├─ Middlewares/
│  ├─ Filters/
│  ├─ Program.cs
│  ├─ appsettings.json
│  └─ GIT.Backend.Api.csproj
│
├─ GIT.Backend.Worker/
│  ├─ Workers/
│  ├─ Consumers/
│  ├─ Program.cs
│  ├─ appsettings.json
│  └─ GIT.Backend.Worker.csproj
│
├─ GIT.Backend.Application/
│  ├─ Services/
│  ├─ UseCases/
│  │  ├─ IngestAnalysisResult/
│  │  ├─ SearchArticles/
│  │  └─ CreateComment/
│  │
│  ├─ DTOs/
│  ├─ Interfaces/
│  └─ GIT.Backend.Application.csproj
│
├─ GIT.Backend.Domain/
│  ├─ Articles/
│  ├─ Analysis/
│  ├─ Comments/
│  ├─ Users/
│  ├─ Common/
│  └─ GIT.Backend.Domain.csproj
│
├─ GIT.Backend.Infrastructure/
│  ├─ Persistence/
│  │  ├─ GitDbContext.cs
│  │  ├─ Entities/
│  │  ├─ Configurations/
│  │  ├─ Repositories/
│  │  └─ Mappers/
│  │
│  ├─ Redis/
│  ├─ External/
│  └─ GIT.Backend.Infrastructure.csproj
│
├─ GIT.Backend.Shared/
│  ├─ Constants/
│  ├─ Exceptions/
│  └─ GIT.Backend.Shared.csproj
│
└─ GIT.Backend.sln
```

---

## 2.3 2차 의존성 방향

2차에서는 프로젝트 참조 방향을 명확히 한다.

```txt
GIT.Backend.Api
  → GIT.Backend.Application
  → GIT.Backend.Infrastructure

GIT.Backend.Worker
  → GIT.Backend.Application
  → GIT.Backend.Infrastructure

GIT.Backend.Application
  → GIT.Backend.Domain

GIT.Backend.Infrastructure
  → GIT.Backend.Application
  → GIT.Backend.Domain

GIT.Backend.Domain
  → 의존성 없음
```

개념적으로는 다음 흐름이다.

```txt
Api ───────────────┐
                   ↓
Worker ─────→ Application ─────→ Domain
                   ↑
Infrastructure ────┘
```

---

## 2.4 API와 Worker 분리

2차에서는 API와 Worker를 별도 실행 프로젝트로 분리한다.

```txt
GIT.Backend.Api
= HTTP 요청 처리
= React 클라이언트 대상 API 제공

GIT.Backend.Worker
= Redis Stream Consumer
= 분석 결과 저장
= Pending Message 재처리
= 장시간 Background Job 처리
```

이 분리가 필요한 시점은 다음과 같다.

```txt
1. API와 Worker를 따로 배포해야 할 때
2. Worker 장애가 API 프로세스에 영향을 주면 안 될 때
3. Redis Consumer 인스턴스를 여러 개 띄워야 할 때
4. API 서버는 가볍게 유지하고 Worker는 별도 리소스를 쓰게 하고 싶을 때
5. Docker Compose 또는 Kubernetes에서 스케일링 단위를 분리하고 싶을 때
```

2차 실행 구조:

```txt
Docker Compose
├─ git-backend-api
├─ git-backend-worker
├─ postgres
├─ redis
├─ crawler
├─ analyzer
└─ frontend
```

---

## 2.5 Domain Model과 EF Entity 분리

2차에서는 1차에서 사용하던 `Domain.Entities`를 그대로 고도화하지 않고, 다음처럼 역할을 분리한다.

```txt
Domain Model
= 비즈니스 개념과 규칙 표현
= EF Core를 모름
= DB 테이블 구조에 직접 종속되지 않음

EF Entity
= DB 매핑 전용 객체
= Infrastructure/Persistence/Entities 하위에 위치
= EF Core Fluent API로 매핑
```

구조 예시:

```txt
GIT.Backend.Domain/
└─ Articles/
   └─ Article.cs

GIT.Backend.Infrastructure/
└─ Persistence/
   ├─ Entities/
   │  └─ ArticleEntity.cs
   ├─ Configurations/
   │  └─ ArticleEntityConfiguration.cs
   └─ Mappers/
      └─ ArticlePersistenceMapper.cs
```

Domain Model 예시:

```csharp
namespace GIT.Backend.Domain.Articles;

public sealed class Article
{
    public long Id { get; }
    public string Url { get; }
    public string Title { get; private set; }

    public Article(long id, string url, string title)
    {
        Id = id;
        Url = url;
        Title = title;
    }

    public void UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required.");

        Title = title.Trim();
    }
}
```

EF Entity 예시:

```csharp
namespace GIT.Backend.Infrastructure.Persistence.Entities;

public sealed class ArticleEntity
{
    public long Id { get; set; }
    public string Url { get; set; } = null!;
    public string Title { get; set; } = null!;
    public DateTime PublishedAt { get; set; }
}
```

Mapper 예시:

```csharp
public static class ArticlePersistenceMapper
{
    public static Article ToDomain(this ArticleEntity entity)
    {
        return new Article(
            entity.Id,
            entity.Url,
            entity.Title
        );
    }

    public static ArticleEntity ToEntity(this Article article)
    {
        return new ArticleEntity
        {
            Id = article.Id,
            Url = article.Url,
            Title = article.Title
        };
    }
}
```

Repository는 EF Entity를 조회한 뒤 Domain Model로 변환해서 Application에 반환한다.

```csharp
public async Task<Article?> GetByIdAsync(long id, CancellationToken ct)
{
    var entity = await _db.Articles
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.Id == id, ct);

    return entity?.ToDomain();
}
```

---

## 2.6 1차 구조에서 2차 구조로 넘어가는 방법

### Step 1. API와 Worker 분리

1차:

```txt
GIT.Backend/
├─ Controllers/
├─ Workers/
└─ Program.cs
```

2차:

```txt
GIT.Backend.Api/
├─ Controllers/
└─ Program.cs

GIT.Backend.Worker/
├─ Workers/
└─ Program.cs
```

---

### Step 2. Application 폴더를 프로젝트로 분리

1차:

```txt
GIT.Backend/Application/
```

2차:

```txt
GIT.Backend.Application/
```

Application에는 다음이 남는다.

```txt
Services
UseCases
DTOs
Interfaces
Application-level Exceptions
```

---

### Step 3. Domain 폴더를 프로젝트로 분리

1차:

```txt
GIT.Backend/Domain/Entities/
```

2차:

```txt
GIT.Backend.Domain/
├─ Articles/
├─ Analysis/
├─ Comments/
└─ Common/
```

이때부터 Domain은 EF Core를 전혀 모르게 한다.

---

### Step 4. Infrastructure 폴더를 프로젝트로 분리

1차:

```txt
GIT.Backend/Infrastructure/
```

2차:

```txt
GIT.Backend.Infrastructure/
├─ Persistence/
├─ Redis/
└─ External/
```

EF Core, Redis, 외부 시스템 연동 구현은 모두 Infrastructure에 위치한다.

---

### Step 5. Domain Entity와 EF Entity 분리

1차:

```txt
Domain/Entities/RawContent.cs
= Domain Entity
= EF Core Entity
```

2차:

```txt
Domain/Articles/Article.cs
= 순수 Domain Model

Infrastructure/Persistence/Entities/ArticleEntity.cs
= EF Core Entity
```

이후 Repository에서 변환한다.

```txt
EF Entity ↔ Domain Model
```

---

# Part 3. 최종 권장 결론

## 3.1 1차 결론

1차에서는 다음 구조로 간다.

```txt
단일 ASP.NET Core 프로젝트
+ Layer 폴더 분리
+ BackgroundService 내장
+ Domain Entity를 EF Entity로 같이 사용
+ Fluent API는 Infrastructure에 배치
+ Controller → Service → Repository → DbContext 흐름 유지
```

즉:

```txt
GIT.Backend.csproj 하나로 시작한다.
```

---

## 3.2 2차 결론

2차에서는 다음 구조로 고도화한다.

```txt
API 프로젝트 분리
Worker 프로젝트 분리
Application 프로젝트 분리
Domain 프로젝트 분리
Infrastructure 프로젝트 분리
Domain Model / EF Entity 분리
Repository에서 Persistence Mapping 수행
```

즉:

```txt
GIT.Backend.Api
GIT.Backend.Worker
GIT.Backend.Application
GIT.Backend.Domain
GIT.Backend.Infrastructure
GIT.Backend.Shared
```

형태로 전환한다.

---

## 3.3 현재 판단

1차에서 중요한 것은 다음이다.

```txt
1. Redis Stream 수신
2. 분석 결과 DB 저장
3. 기사/분석/지역 데이터 조회 API
4. React + Leaflet 화면 연동
5. Docker Compose로 전체 실행
```

이것이 완성된 뒤에 구조 개선을 2차로 진행한다.

오히려 현재 단계에서 구조를 과하게 분리하면 다음 문제가 생긴다.

```txt
1. 프로젝트 참조 관리 부담 증가
2. DTO/Entity/Model 위치 논쟁 증가
3. 실제 기능 구현 속도 저하
4. AI/Crawler/Redis/React/Leaflet 학습 부담과 충돌
5. 완성 전에 구조 작업만 반복할 가능성 증가
```

따라서 최종 방침은 다음과 같다.

```txt
1차: 단순하게 만든다.
2차: 완성된 시스템을 기준으로 분리한다.
```

---

# Appendix. 1차 최소 의존성 규칙

1차에서 단일 프로젝트로 가더라도 다음 규칙은 지킨다.

## 허용 흐름

```txt
Controller → Application Service
Application Service → Repository Interface
Repository Implementation → GitDbContext
Worker → Application Service
Domain Entity → 외부 기술 의존 없음
```

## 금지 흐름

```txt
Controller → GitDbContext 직접 접근
Controller → Redis 직접 접근
Domain → EF Core 참조
Domain → Redis 참조
Application → StackExchange.Redis 직접 참조
Application → Npgsql 직접 참조
```

## 실용적 예외

1차에서는 단일 프로젝트이므로 컴파일 레벨에서 의존성을 완전히 막을 수는 없다. 대신 코드 리뷰 기준으로 다음을 유지한다.

```txt
1. Controller는 얇게 유지한다.
2. Service에 기능 흐름을 둔다.
3. Repository는 DB 접근만 담당한다.
4. Entity는 상태와 최소한의 비즈니스 규칙을 가진다.
5. Redis Consumer는 메시지 수신 후 Application Service에 위임한다.
```
