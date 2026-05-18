# Backend AGENTS.md

## 1. 목적

이 문서는 G.I.T Backend 개발 Agent의 서비스 단위 작업 지침이다. Backend Agent는 ASP.NET Core API, Worker, EF Core, PostgreSQL, Redis Streams 연동 작업을 수행한다.

Root `AGENTS.md`의 Monorepo 공통 원칙을 우선 따르며, 이 문서는 Backend 내부 기준만 다룬다.

---

## 2. Backend 책임

Backend는 G.I.T 시스템의 데이터 정합성 중심이다.

주요 책임은 HTTP API 제공, Redis Streams 메시지 소비, PostgreSQL 저장/조회, EF Core CodeFirst 기반 DB 모델 관리, Article/Analysis/Location 데이터 정합성 보장, 중복 데이터 방지, Frontend 조회 API 제공이다.

Crawler와 Analyzer는 PostgreSQL에 직접 접근하지 않는다. DB 쓰기와 최종 정합성 책임은 Backend가 가진다.

---

## 3. 개발 방향

현재 우선순위는 아키텍처 과시가 아니라 MVP 완성이다.

우선순위는 Redis Streams 메시지 정상 소비, 분석 결과 PostgreSQL 저장, 중복 Article 방지, Frontend 조회 API 제공, Docker Compose 실행 상태 유지이다.

Clean Architecture를 강하게 적용하지 않는다. 다만 Layer 간 의존성 방향과 책임 분리는 유지한다.

---

## 4. 아키텍처 기준

다음 구조는 기본값이 아니다.

- 모든 기능의 UseCase 클래스화
- 모든 Service의 Interface화
- EF Entity와 Domain 객체의 무조건 분리
- Repository Pattern 일괄 적용
- MediatR, CQRS, Domain Event, DDD Aggregate 도입

실제 복잡도, 테스트 필요성, 중복 흐름이 생겼을 때만 검토한다.

MVP 단계에서는 단순한 Layered Architecture를 우선한다.

```text
API Controller → Application Service → Infrastructure / DbContext → PostgreSQL
Redis Streams → Worker → Application Service → Infrastructure / DbContext → PostgreSQL
```

권장 의존 방향은 `API / Worker → Application → Infrastructure / EF Core`이다.

이론적 분리를 위해 프로젝트를 무리하게 쪼개지 않는다. Namespace, Folder, Class 책임 분리부터 우선한다.

---

## 5. Layer별 책임

API Layer는 Request DTO 수신, 기본 Validation, Application Service 호출, Response DTO 반환, HTTP Status Code 결정을 담당한다. 직접적인 DB 저장, Redis 처리, 복잡한 비즈니스 로직, 트랜잭션 직접 제어를 하지 않는다.

Worker Layer는 Redis 연결, Consumer Group 기반 메시지 읽기, 메시지 역직렬화, Application Service 호출, 성공 시 ACK, 실패 로그, 재처리 가능성 유지를 담당한다. 복잡한 DB 저장 로직을 직접 작성하지 않고, 실패 메시지를 무시하지 않으며, DB 저장 전 ACK하지 않는다.

Application Layer는 Article/Analysis/Location 저장 흐름, 조회 조건 처리, 중복 판단, 트랜잭션 단위 조정, DTO와 Entity 간 변환을 담당한다. Service가 커지면 기능 단위로 분리하되, 단순 CRUD까지 무조건 UseCase로 분리하지 않는다.

Infrastructure Layer는 EF Core DbContext, Entity Configuration, PostgreSQL 접근, Redis Client 구현, 외부 라이브러리 연동, Migration 관리를 담당한다. Infrastructure 세부 구현이 Controller로 직접 새어나가지 않게 한다. 모든 DB 접근을 Repository로 감싸는 것은 필수가 아니다.

---

## 6. UseCase / Interface / Domain 분리 기준

UseCase는 하나의 Service 메서드가 여러 책임을 가지거나, 같은 흐름이 API와 Worker에서 함께 사용되거나, 트랜잭션 단위가 복잡하거나, 독립 테스트가 필요하거나, 작업 의도가 불명확해졌을 때만 검토한다.

Service Interface는 구현체 교체 가능성이 실제로 있거나, 테스트 Mocking 필요성이 명확하거나, API와 Worker가 같은 계약에 의존해야 하거나, 외부 기술 구현과 Application 계약을 분리해야 할 때만 도입한다.

Domain 객체와 EF Entity는 기본적으로 강제 분리하지 않는다. MVP 단계에서는 EF Entity가 DB 저장 구조와 간단한 도메인 상태를 함께 표현할 수 있다. API DTO와 Redis Message DTO는 Entity와 분리한다.

---

## 7. DTO / EF Core 규칙

DTO는 외부 계약이다. Entity를 API Response나 Redis Message로 직접 사용하지 않는다. DTO는 HTTP Request DTO, HTTP Response DTO, Redis Message DTO, EF Entity를 구분한다.

DTO 변경 시 Frontend, Crawler, Analyzer와의 호환 영향을 확인한다.

Backend의 DB Schema 기준은 EF Core CodeFirst이다.

작업 순서는 Entity 작성/수정 → DbContext 반영 → Fluent API 설정 → Migration 생성 → DB 반영이다.

SQL 파일을 먼저 작성하고 Entity를 맞추는 방식은 기본으로 사용하지 않는다.

Entity 작성 시 PK, FK, Required/Nullable, Unique Index, CreatedAt/UpdatedAt, Delete 정책, `jsonb` 컬럼 의미, Provider Snapshot 필요 여부를 명확히 한다.

Article URL 등 중복 방지 기준은 Application 체크보다 DB 제약 조건을 우선한다.

---

## 8. Redis Streams / Transaction 규칙

Redis Streams는 서비스 간 이벤트 전달 계층이며 Source of Truth가 아니다. 최종 저장 상태는 PostgreSQL을 기준으로 판단한다.

ACK는 DB 저장 성공 이후 수행한다.

```text
메시지 수신 → 역직렬화 → 유효성 검증 → Application Service 호출 → DB 저장 성공 → Redis ACK
```

DB 저장 실패 시 ACK하지 않는다.

Worker는 같은 메시지를 여러 번 처리해도 데이터가 깨지지 않도록 작성한다. Unique Index로 최종 중복을 방어하고, 중복 Article은 정상 처리 또는 Skip 처리하며, 재처리 시 같은 결과가 되도록 Idempotent하게 작성한다.

트랜잭션은 여러 테이블 변경이 하나의 의미를 가질 때 사용한다. 예: `Article 저장 + ArticleAnalysis 저장 + AnalysisLocation 저장`

트랜잭션 성공 후 Redis ACK를 수행한다.

---

## 9. 금지 사항

Backend Agent는 사용자 요청 없이 다음 작업을 수행하지 않는다.

- Clean Architecture 강제 적용
- 모든 기능의 UseCase 클래스화
- 모든 Service의 Interface화
- 모든 Domain 객체와 EF Entity 분리
- Repository Pattern 일괄 도입
- MediatR, CQRS, DDD Aggregate 도입
- 프로젝트 구조 대규모 변경
- DB Schema 대규모 변경
- Redis Message Schema 변경
- API Response 구조 변경
- Python 서비스 코드 수정
- Frontend 코드 수정
- Docker Compose 구조 변경
- 사용하지 않는 공통 라이브러리 생성

---

## 10. 판단이 애매할 때

MVP 동작과 DB 정합성을 우선한다. Layer 간 책임 분리는 유지하되 Clean Architecture 패턴은 강제하지 않는다. UseCase, Interface, Domain 분리는 필요해졌을 때만 적용한다. Redis ACK는 DB 저장 성공 이후로 둔다. 외부 계약 변경은 최소화하고 YAGNI 원칙을 따른다.
