## 개요
이 문서는 C#/ASP.NET Core 백엔드 코드 작성 시 스타일 지침을 설명합니다.

## 세부지침
- 함수를 작성할 때 builder 패턴식의 인자 하나당 개행 한번을 넣는 방식을 지양하라. 인자가 한 줄에 3~4개 이상으로 길어지면 개행을 고려하라.
ex)
// 개행 필요없는 경우
public CrawlerWorker(IConnectionMultiplexer redis,
                     IServiceScopeFactory scopeFactory,
                     ILogger<CrawlerWorker> logger)
...
// 이정도 길이는 한줄에 작성
public CrawlerWorker(IConnectionMultiplexer redis, IServiceScopeFactory scopeFactory, ILogger<CrawlerWorker> logger)

// 인자를 늘어놓았을 때 두 줄 이상으로 자동 개행될 정도로 길어지면 개행 고려
public CrawlerWorker(IConnectionMultiplexer redis, IServiceScopeFactory scopeFactory, ILogger<CrawlerWorker> logger, ASDService asdService, BBBService bbbService, DBContext dbContext, ...blablablablablablablablablablablablablablablablablablablablablabla)

// 모든 인자를 개행으로 구분할 필요는 없고 2~3개 인자씩 묶어서 개행
public CrawlerWorker(IConnectionMultiplexer redis, IServiceScopeFactory scopeFactory,
                     ILogger<CrawlerWorker> logger, ASDService asdService, BBBService bbbService,
                     DBContext dbContext, ...blablablablablablablablablablablablablablablablablablablablablabla)