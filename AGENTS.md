## 개요
GIT 프로젝트는 뉴스/이슈 데이터를 Crawling, AI Analyze하여 지역 정보와 연결하여
지도에 시각화 하는 GIS 서비스 이다.

시스템 구성:
- Backend: ASP.NET Core API + Worker
- Frontend: React + Leaflet
- Crawler Service: Python
- AI Analyzer: Python
- DataBase: PostgreSQL
- EventBroker: Redis Streams
- Infra: Docker Compose

## 코드작업 지침
실용적이고 클린한 아키텍쳐를 추구하되 과하게 일반화된 엔터프라이즈급 설계를 경계하여 작업하라.
클린 코드, 클린 아키텍쳐를 기본 원칙으로 준수하라.

- 명확한 책임 경계를 가지고 설계하라
ex) Layer 의존성 경계를 지키고 강한 결합을 지양하라.
- God Object를 설계하지 말고 용도에 따라 적절하게 모듈을 분리하라.
- YAGNI("You Aren't Gonna Need It") 원칙을 준수하라.
- 명시적으로 요청되었거나 기술적으로 필요한 경우가 아니면 광범위한 리팩토링 또는 구조적 변경을 수행하지 마라.
- 각 서비스에서 사용되는 언어와 프레임워크의 최신 표준 개발방법을 적용하라.

## Repository 참조 지침
이 프로젝트는 Mono Repo로 구성되어 있다.
에이전트는 API 계약, 이벤트 계약, 데이터 흐름, 종속성 또는 영향 분석을 위해 다른 서비스를 읽을 수 있다.
하지만 명시적으로 요청되었거나 기술적으로 필요한 경우가 아니면 다른 서비스를 참조하는것은 자제하고 특히 명확한 요청이 있지 않으면 다른 서비스를 수정하지 마라.

## 작업 순서
- 문제를 작은 Task로 분리하고 단계별 체크리스트를 작성하여 작업하라.