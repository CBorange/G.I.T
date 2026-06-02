## 개요
이 문서는 DB Schema 설계 수정 작업 또는 테이블 구조 참조가 필요한 경우 작업 지침에 대해 설명하는 문서이다.

## DB 설계 지침
- DB 설계 원본은 docs/DB/DB_schema.sql 파일을 최초 근거로 참고하라.
- 실제 구현 기준은 백엔드 EF Entity, Fluent API, EF Migration 이다.

## 주요 테이블 관계 및 설계 근거
이 문단에선 주요 테이블의 설계 근거 및 실제 코드와 연관성을 설명한다.

### SourceProvider
- 크롤러 구현체와 1:1 대응되는 단위이다.
- 실제 크롤링 URL은 CrawlTarget이 가진다. SourceProvider와 CrawlTarget은 1:N 관계이다.
- base_url은 참조용이다. 크롤러 구현체 로직에서 정확히 동일한 url을 호출할 필요는 없다.

### SourceCategory
- 크롤링 소스의 데이터 종류를 선언해놓은 테이블이다.
- 크롤링 및 AI 분석 시의 데이터 카테고리 분류를 위한 기준이다.

### CrawlTarget
- 실제 수집 대상 URL/카테고리/엔드포인트 단위이다.
- entry_url은 참조용이다. 크롤러 구현체 로직에서 정확히 동일한 url을 호출할 필요는 없다.

### RawContent
- 크롤러가 수집한 원문 데이터 저장 테이블이다.
- 동일한 source_url을 가진 데이터는 중복 저장할 수 없다.

### AnalyzeJob
- raw_content를 분석하기 위한 작업 상태를 저장하는 테이블이다.
- raw_content와 1:1 관계이다. ai analyze 실행 시 마다 analyze_job에 작업 현황을 기록해야 한다.

## AnalyzerProvider
- AI 분석기 구현체와 1:1 대응되는 단위이다.
- 분석 작업에서 사용될 모델 api endpoint url과 모델 정보를 저장한다.

### AnalysisRoute
- 크롤링 작업기(SourceProvider) 혹은 크롤링 소스 카테고리(SourceCategory)와 매칭되는 AnalyzerProvider를 연결해놓은 라우팅 테이블이다.
- 크롤링 데이터 원문의 AI 분석 작업 대기(Pending)으로 지정할 때 우선순위에 따라 매칭되는 AnalyzerProvider를 조회할 수 있도록 SourceProviderId, SourceCategoryId를 FK로 가진다. 우선순위는 다음과 같다.
  1. SourceProviderId, SourceCategoryId가 모두 매칭되는 경우
  2. SourceProviderId가 매칭되는 경우
  3. SourceCategoryId가 매칭되는 경우
  4. 위 조건들에 모두 매칭되지 않는 경우
우선순이 4번 매칭조건에 모두 만족하지 못할 경우 Default Route를 태운다. IsDefault가 True인 record로 route 한다.
- is_default 가 True인 Case는 테이블전체에서 하나의 record만 허용한다. 즉, Default record는 같은 테이블에 2개 이상 존재할 수 없다.
- is_default 가 True인 record는 source_provider_id, source_category_id 가 모두 null이어야 한다.(특별히 매칭되는 provider, category가 없는 경우에 route 되는 record 이기 때문에)

### AnalyzedContents
- 크롤링 데이터 원문(RawContents)의 정보를 AI가 요약 및 키워드/지역 추출한 정보를 저장하는 테이블이다.
- User가 조회할 수 있는 정제된 크롤링 데이터 이다.
- analyze_job 테이블을 참조하여 분석 후 분석 상태를 analyze_job 테이블에 반영되어야 한다.
- 분석 후 confidence(분석 신뢰도)의 값을 0~1 사이 값으로 저장해야 한다. confidence_reason도 함께 저장되어야 한다.
- confidence(신뢰도)는 항상 0~1 사이의 값이어야 한다.