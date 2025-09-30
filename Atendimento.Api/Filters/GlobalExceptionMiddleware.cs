using Atendimento.Api.Exceptions;
using Atendimento.Api.Models.Common;
using System.Net;
using System.Text.Json;

namespace Atendimento.Api.Filters
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

            HttpStatusCode status;
            string code;
            string message;
            object? details = null;

            switch (ex)
            {
                case ConflictException ce:
                    status = HttpStatusCode.Conflict;
                    code = ce.Code;
                    message = ce.Message;
                    details = ce.Details;
                    break;

                case UnauthorizedAccessException:
                    status = HttpStatusCode.Unauthorized;
                    code = "unauthorized";
                    message = "Não autorizado.";
                    break;

                case System.Security.SecurityException:
                    status = HttpStatusCode.Forbidden;
                    code = "forbidden";
                    message = "Acesso negado.";
                    break;

                default:
                    status = HttpStatusCode.InternalServerError;
                    code = "internal_error";
                    message = "Erro inesperado.";
                    break;
            }

            ctx.Response.StatusCode = (int)status;
            ctx.Response.ContentType = "application/json";

            var body = JsonSerializer.Serialize(
                ApiResponse.Fail(code, message, null, traceId));

            return ctx.Response.WriteAsync(body);
        }
    }
}
