using Atendimento.Models.Common;
using System.Net;

namespace Atendimento.Filters
{
    public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        public async Task Invoke(HttpContext ctx)
        {
            try
            {
                await next(ctx);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception");
                await WriteError(ctx, ex);
            }
        }

        private static Task WriteError(HttpContext ctx, Exception ex)
        {
            var traceId = ctx.TraceIdentifier;

            var (status, code, message) = ex switch
            {
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "unauthorized", "Não autorizado."),
                System.Security.SecurityException => (HttpStatusCode.Forbidden, "forbidden", "Acesso negado."),
                _ => (HttpStatusCode.InternalServerError, "internal_error", "Erro inesperado.")
            };

            ctx.Response.ContentType = "application/json";
            ctx.Response.StatusCode = (int)status;

            var body = System.Text.Json.JsonSerializer.Serialize(
                ApiResponse.Fail(code, message, null, traceId));

            return ctx.Response.WriteAsync(body);
        }
    }
}
