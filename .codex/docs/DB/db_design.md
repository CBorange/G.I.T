## 개요
이 문서는 DB Schema 설계 수정 작업 또는 테이블 구조 참조가 필요한 경우 작업 지침에 대해 설명하는 문서이다.

## DB 설계 지침
- DB 설계 원본은 docs/DB/DB_schema.sql 파일을 최초 근거로 참고하라.
- 실제 구현 기준은 백엔드 EF Entity, Fluent API, EF Migration 이다.
- **Entity/Fluent API 수정 시 ERD/DB 설계 문서와 불일치가 생기면 반드시 문서를 함께 갱신하거나 사용자에게 확인한다.**
- DB_Schema.sql은 초기 설계 참고용이며, 현재 구현과 다를 수 있으므로 단독 기준으로 삼지 않는다.

## 주요 테이블 관계 및 설계 근거
이 문단에선 주요 테이블의 설계 근거 및 실제 코드와 연관성을 설명한다.

### SourceProvider
- 크롤러 구현체와 1:1 대응되는 단위이다.
- 실제 크롤링 URL은 CrawlTarget이 가진다. SourceProvider와 CrawlTarget은 1:N 관계이다.
- base_url은 참조용이다. 크롤러 구현체 로직에서 정확히 동일한 url을 호출할 필요는 없다.

### CrawlTarget
- 실제 수집 대상 URL/카테고리/엔드포인트 단위이다.
- entry_url은 참조용이다. 크롤러 구현체 로직에서 정확히 동일한 url을 호출할 필요는 없다.

### RawContent
- 크롤러가 수집한 원문 데이터 저장 테이블이다.