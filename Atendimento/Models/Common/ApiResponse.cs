namespace Atendimento.Api.Models.Common
{
    public record ApiError(string Code, string Message, object? Details = null);

    public record ApiResponse(
        bool Success,
        object? Data,
        ApiError? Error,
        string TraceId,
        DateTime TimestampUtc
    )
    {
        public static ApiResponse Ok(object? data, string traceId) =>
            new(true, data, null, traceId, DateTime.UtcNow);

        public static ApiResponse Fail(string code, string message, object? details, string traceId) =>
            new(false, null, new ApiError(code, message, details), traceId, DateTime.UtcNow);
    }
}
