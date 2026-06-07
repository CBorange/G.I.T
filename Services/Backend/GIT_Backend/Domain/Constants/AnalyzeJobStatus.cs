namespace GIT_Backend.Domain.Constants
{
    public enum AnalyzeJobStatus : byte
    {
        /// <summary>
        /// 분석 대기중
        /// </summary>
        Pending = 1,

        /// <summary>
        /// Analyzer에게 분석 요청 이벤트 발행 완료
        /// </summary>
        Dispatched = 2,

        /// <summary>
        /// 분석 성공
        /// </summary>
        Succeeded = 3,

        /// <summary>
        /// 분석 실패 (재시도 가능)
        /// </summary>
        Failed = 4,

        /// <summary>
        /// 최대 재시도 초과 또는 복구 불가
        /// </summary>
        Dead = 5,
    }
}
