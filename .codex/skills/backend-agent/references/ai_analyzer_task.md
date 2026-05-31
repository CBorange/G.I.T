## 개요
이 문서는 RawContents 데이터 추가 또는 AI 분석 실행과 관련된 작업을 수행할 때 참조하는 Context 문서이다.

## analyze_job status값 상태 전이표
- AnalyzeJobStatus enum class 참조.
- 상태 전이 제약조건
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