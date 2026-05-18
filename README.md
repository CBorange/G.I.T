# 🗺️ G.I.T - Geospatial Issue Tracker

> 뉴스·이슈 데이터를 수집하고 AI로 분석한 뒤, 지역 단위로 분류하여 지도 위에 시각화하는 이슈 트래킹 서비스

---

## 서비스 개요

**G.I.T(Geospatial Issue Tracker)** 는 서울 지역의 뉴스·공공 이슈를 수집하고, AI 분석을 통해 요약·키워드·지역 정보를 추출한 뒤 지도 기반으로 시각화하는 서비스입니다.

지역 이슈를 **공간 정보(Geospatial Data)** 와 연결하여 지도 기반으로 확인할 수 있게 제공합니다.

### 핵심 기능

| 기능 | 설명 |
|---|---|
| 📰 이슈 수집 | 외부 뉴스/이슈 데이터를 주기적으로 수집 |
| 🧹 데이터 정규화 | Source별 Crawling 데이터 도메인 포맷으로 변환 |
| 🧠 AI 분석 | 기사 요약, 키워드 추출, 지역명 추출 |
| 📍 지역 매핑 | 추출된 지역 정보를 서울 행정구역 기준으로 매핑 하여 지도 마킹 |
| 🔎 이슈 조회 | 기사 목록, 상세 내용, 분석 결과 조회 |
| 🔄 서비스 통신 | Redis Streams 기반 Event Broker 방식 서비스간 통신 구현 |

---

## 시스템 구조

각 서비스는 독립적인 책임을 가지고 메인 백엔드를 중심으로 Redis Streams 기반 이벤트 통신을 사용합니다.

![시스템 아키텍처 도식화 이미지](./Docs/Images/서비스_아키텍쳐.png)

## 서비스별 역할

| Service | Tech | Responsibility |
|---|---|---|
| **Backend** | ASP.NET Core | Application Service + Orchestrator + Data Authority |
| **Backend Worker** | ASP.NET Core BackgroundService | Redis Stream Consumer, Crawler/Analyzer 데이터 Validate, PostgreSQL DB 저장 |
| **Crawler** | Python | 외부 뉴스/이슈 수집, 1차 정규화, RawContents Event 발행 |
| **Analyzer** | Python | AI 요약, 키워드 추출, 지역명 추출, 분석 Event 발행 |
| **Frontend** | React, Leaflet | 지도 기반 이슈 시각화, 기사 목록/상세 UI |
| **PostgreSQL** | PostgreSQL | Database |
| **Redis Streams** | Redis | 서비스 간 비동기 이벤트 파이프라인 |

---

## 데이터 흐름

```text
[1] Crawler
    └─ 뉴스/이슈 데이터 수집

[2] Redis Streams
    └─ raw content event 발행

[3] AI Analyzer
    └─ 요약 / 키워드 / 지역명 분석

[4] Redis Streams
    └─ 크롤링, AI 분석 결과 Event 발행

[5] Backend Worker
    └─ 이벤트 소비, Data Validation, DB 저장

[6] Backend API
    └─ 기사/분석 결과 조회 API 제공

[7] Frontend
    └─ 지도 기반 이슈 시각화
```

---

## DB 설계, ERD

DB 설계는 EF Core CodeFirst 방식으로 진행했습니다.
Crawler와 Analyzer는 DB에 직접 접근하지 않고, Backend Worker가 이벤트를 소비하여 최종 데이터를 저장합니다.

![시스템 아키텍처 도식화 이미지](./Docs/Images/DB_ERD_v3.png)

---

## 배포 전략

배포는 로컬 실행 환경에서 시작해 Docker Compose 기반 통합 환경을 구성하고, 이후 클라우드 환경으로 점진적으로 확장합니다.

```mermaid
flowchart LR
    Local[Local Development] --> Compose[Docker Compose]
    Compose --> Cloud[Cloud Deployment]
```

| 단계 | 목표 | 설명 |
|---|---|---|
| 1 | Local Development | 개별 서비스 로컬 실행 및 기능 검증 |
| 2 | Docker Compose | Backend, Worker, Crawler, Analyzer, PostgreSQL, Redis 통합 실행 |
| 3 | Cloud Deployment | API/Worker/Frontend/DB/Redis 클라우드 배포 |

---

## 🛣️ Roadmap

세부 작업은 Issue 또는 별도 문서에서 관리하고, README에서는 큰 목표 단위만 추적합니다.

| Phase | 목표 | Status |
|---|---|---|
| 1️⃣ MVP | 서울 지역 대상, 1개 카테고리 크롤링 기반 End-to-End 동작 구현 | in-progress |
| 2️⃣ Refactoring | Clean Architecture 적용 Backend 구조 개선 | - |
| 3️⃣ Expansion | 서울 지역 N개 카테고리 확장 및 지역 기준 분석 고도화 | - |

---

## 🧪 Tech Stack

| Area | Stack |
|---|---|
| Backend | C#, ASP.NET Core, EF Core, BackgroundService |
| Frontend | React, TypeScript, Leaflet |
| Crawler | Python, Web Crawling |
| Analyzer | Python, LLM Agent 분석기 Pipeline |
| Database | PostgreSQL |
| Event Broker | Redis Streams |
| Infra | Docker, Docker Compose, Nginx, Oracle Cloud(예정) |
| DevOps | GitHub Actions |

---

## 📝 개발 의사결정 기록

이 문단은 G.I.T의 기획, 시스템 아키텍처, 서비스 간 통신, 백엔드/프론트엔드 설계, 배포 환경 구성 과정에서 발생한 주요 의사결정을 기록한 섹션입니다.

<details>
<summary><strong>기획</strong></summary>

### [프로젝트 개요]

지역 뉴스나 사사로운 지역 이슈 들을 지도 기반으로 쉽게 확인할 수 있는 서비스가 존재한다면
어떨까 라는 생각으로 개발하게 되었습니다.

또한 개발 학습 차원에서 기존의 GIS 기반 웹서비스 개발 경험을 바탕으로 **웹 크롤링 + AI Pipeline + 웹 서비스 아키텍쳐 설계**를 경험하고
AI Agent를 활용한 **AI-Assisted 개발 Workflow 학습**을 목적으로 진행하였습니다.

### [1차/2차 개발계획]

지역 기반 이슈 소스 탐색 + 웹 크롤링 후 포맷팅 -> AI 분석을 통한 키워드 추출 이라는 주요 기능 구현 및 실증을 우선목표로 1차 개발을 진행하고

2차 개발에서 Architecture 개선 등 고도화 하는 전략으로 방향을 정했습니다.

![시스템 아키텍처 도식화 이미지](./Docs/Images/프로젝트_로드맵.png)

2차 아키텍쳐 개선 이후 Source 카테고리를 추가하고 서비스 지역을 확대하는것이 목표입니다.

</details>


