---
name: backend-agent
description: Backend service development guidance for the G.I.T monorepo. Use when working on Backend service tasks in this repository.
---

## 문서 참조 순서
아래 순서로 문서를 먼저 참조하고 이 skill 문서를 읽으시오.
1. `Root/AGENTS.md`
2. `docs/Backend/aspnet_style.md`
2. 이 스킬 문서

작업 내용에 따라 작업 수행 전에 연관된 문서를 먼저 참조하시오.
- RawContents 추가, AI 분석 Event 발행, AI 분석 결과 저장 등 크롤링 데이터 파싱 -> 분석 계열 작업 시
  - 'references/ai_analyzer_task.md'

## 작업 문맥
당신의 역할은 G.I.T 프로젝트의 백엔드 개발 에이전트 역할을 수행하는것이다.

작업 디렉터리:
{ProjectRoot}/Services/Backend/GIT_Backend
명시적으로 지정하지 않는 한 모든 상대 경로는 이 디렉터리를 기준으로 합니다.
* 현제 프로젝트는 1차 MVP 단계이므로 과도한 일반화나 추상화는 지양하고 실용적이고 클린한 아키텍쳐를 추구하라.
* 패턴화를 위한 무조건 적인 Interface 분리, Layer간 과도한 의존성 규칙 적용, 불필요한 추상화는 피하라.

## 기술스택
- C#/ASP.NET Core (.NET 10.0)
- Entity Framework Core, CodeFirst, PostgrSQL, Fluent API 기준 설계
- Redis Consumer/Producer Background Worker

## 아키텍처 규칙

클린 아키텍처 방향은 준수하되 객체와 레이어를 과도하게 분리하지 않는다.  
기본 작업 단위는 Service Layer이며, UseCase/Repository/Command/Query는 복잡도가 실제로 생긴 경우에만 분리한다.  
DTO는 외부 Payload 계약과 1차 형식 검증 단위이고, Controller는 검증된 요청을 내부 실행 요청으로 변환하는 HTTP 경계다.

### Controllers
- API Endpoint, Request DTO Binding/Validation, Root 예외 응답, Response DTO 반환을 담당한다.
- 비즈니스 로직과 DB 조회를 직접 수행하지 말고, 필요 시 Request DTO를 Input/Command/Query로 변환해 Service/UseCase에 전달한다.

### Application
- Service, UseCase, Input/Command/Query, DTO, Result, Transaction, 비즈니스 흐름 조합을 담당한다.
- 기본은 Service로 작성하고, 명확한 단일 업무 흐름/트랜잭션/여러 Repository 조합이 생길 때만 UseCase로 분리한다.

#### DTO 생성 규칙
- Response DTO는 positional record 우선.
- Request DTO는 validation attribute가 많으면 property init record 우선.
- DTO에 상태 변경 메서드는 넣지 않는다.
- DTO가 복잡한 행동을 갖기 시작하면 DTO가 아니라 Domain/Input/Mapper 책임인지 의심한다.

### Domain
- Entity, Value Object, Domain Service, Domain Event, 핵심 비즈니스 상태와 규칙을 담당한다.
- EF Entity와 Domain 객체를 겸용할 수 있으며, 도메인 규칙이 커질 때 분리한다.

### Infra
- DbContext, EF Fluent API Configuration, Repository 구현체, Log, 환경변수 Load, 외부 시스템 연동을 담당한다.
- Service가 DbContext를 직접 사용할 수 있고, 조회 코드 반복/복잡한 조건/테스트 부담이 생기면 Repository로 분리한다.