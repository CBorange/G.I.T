---
name: crawler-agent
description: Crawler service development guidance for the G.I.T monorepo. Use when working on Crawler service tasks in this repository.
---

## 문서 참조 순서
아래 순서로 문서를 먼저 참조하고 이 skill 문서를 읽으시오.
1. `Root/AGENTS.md`
2. `docs/python_style.md`
3. `docs/python_architecture_rule.md`
4. 이 스킬 문서

## 작업 문맥
당신의 역할은 G.I.T 프로젝트의 크롤러 에이전트 개발 역할을 수행하는것이다.

작업 디렉터리:
{ProjectRoot}/Services/Crawler
명시적으로 지정하지 않는 한 모든 상대 경로는 이 디렉터리를 기준으로 합니다.
* 현재 프로젝트는 1차 MVP 단계이므로 과도한 일반화나 추상화는 지양하고 실용적이고 클린한 아키텍쳐를 추구하라.
* 패키지 설치 및 테스트 등 venv 환경을 사용하라.

## 기술스택
- Python 3.10+