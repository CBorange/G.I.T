---
name: crawler-agent
description: Crawler service development guidance for the G.I.T monorepo. Use when working on Crawler service tasks in this repository.
---

## 문서 참조 순서

1. 먼저 `Root/AGENTS.md` 를 참조하시오. 그런 다음 이 스킬 문서를 읽으시오.
2. 코드 작업에 들어가기 전에 `docs/python_style.md` 를 참조하여 Python coding style guidelines를 따르세요.

## 작업 문맥
당신의 역할은 G.I.T 프로젝트의 크롤러 에이전트 개발 역할을 수행하는것이다.

작업 디렉터리:
{ProjectRoot}/Services/Crawler
명시적으로 지정하지 않는 한 모든 상대 경로는 이 디렉터리를 기준으로 합니다.
* 현재 프로젝트는 1차 MVP 단계이므로 과도한 일반화나 추상화는 지양하고 실용적이고 클린한 아키텍쳐를 추구하라.
* 패키지 설치 및 테스트 등 venv 환경을 사용하라.

## 기술스택
- Python 3.10+
- HTTP API Cleint, BeautifulSoup4, Logging

## 아키텍처 설명
- 함수형 프로그래밍 패러다임을 따르며, 각 함수는 명확한 입력과 출력을 가진다.
- 모듈은 기능별로 분리하고 상태관리 패턴을 자제하라.
- 필요한 상태(예를 들어 requests.session) 같은 것들은 main.py 진입점에서 생성하여 각 서비스로 인자를 통해 전파한다.
- API Response, Cralwer Result 등과 같은 데이터 구조는 models 디렉터리에 dataclass로 정의하여 사용하라.