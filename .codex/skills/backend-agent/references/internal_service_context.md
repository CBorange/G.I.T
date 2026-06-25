## 개요
이 문서는 ASP.NET 백엔드를 중심으로 내부 서비스(크롤러, AI 분석기)들의 관계 및 연동 작업 진행 시 참고사항에 대해 정리한 문서이다.

## 서비스 데이터 흐름
각각의 서비스는 백엔드를 중심으로 Redis Stream(EventBroker)를 통해 통신한다.
- Crawler Service
  1. 백엔드에 SourceProvider 정보 요청, 유효한 ServiceProvider 목록 Loop 하면서 크롤링 진행
  2. SourceProvider 와 1:1 매칭하여 크롤러 구현체 생성 및 크롤링 진행. 생성한 크롤링 데이터(RawContents) Redis에 발행
- Analyzer Service
  1. Redis Consumer Worker Loop가 Pending 상태의 AnalyzerJob 정보 Consume. AnalyzerProvider 정보 기준으로 분석기 구현체 생성 및 실행
  2. LLM API 호출하여 RawContents 요약 및 키워드 추출. AnalyzedContents 신규 데이터 Redis Stream에 발행
- Backend Service(Worker 단위 설명)
  1. Crawler Data Consumer: Crawler Service가 발행한 RawContents 데이터 Validation/Save. AnalyzeJob 분석 대기 데이터 Insert
  2. AnalyzeJob Dispatcher: AnalyzeJob 테이블에서 Pending 상태의 Task Redis Stream에 이벤트 발행. (AI 분석기 실행 요청에 해당)
  3. AnalyzedContents Consumer: AI 분석기 서비스가 발행한 분석된 데이터 Consume. Validation 및 Save. AnalyzeJob Done or failed 처리

위 와 같이 최초 서비스 Boot용 데이터 요청 제외한 REST API 연동을 자제하고 Redis Stream을 통한 Consumer/Publisher 패턴을 기반으로 데이터를 송수신 하는 구조로 동작한다.
Analyzer Service는 백엔드가 정제한 요청만 받아서 처리한다. 자체적으로 AnalyzeJob 상태를 알지 못하고 요청하는 데이터만 처리한다.

## analyze_job status값 상태 전이표
analyze_job 발행, 참조 등 작업 시 analyze_job 테이블의 status값의 상태 전이 규약에 대한 설명이다.
- AnalyzeJobStatus enum class 참조.
- 상태 전이 규칙은 아래와 같다.
Pending
  -> Dispatched
Dispatched
  -> Succeeded
  -> Failed
Failed
  -> Pending
  -> Dead
Succeeded
  -> Terminal
Dead
  -> Terminal