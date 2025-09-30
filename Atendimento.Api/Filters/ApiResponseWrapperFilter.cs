using Atendimento.Api.Models.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Atendimento.Api.Filters
{
    public class ApiResponseWrapperFilter : IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is ObjectResult { Value: ApiResponse })
            {
                await next();
                return;
            }

            var traceId = context.HttpContext.TraceIdentifier;

            switch (context.Result)
            {
                case ObjectResult obj:
                    context.Result = new ObjectResult(ApiResponse.Ok(obj.Value, traceId))
                    {
                        StatusCode = obj.StatusCode ?? StatusCodes.Status200OK
                    };
                    break;

                case EmptyResult:
                    context.Result = new ObjectResult(ApiResponse.Ok(null, traceId))
                    {
                        StatusCode = StatusCodes.Status204NoContent
                    };
                    break;

                case ContentResult content:
                    context.Result = new ObjectResult(ApiResponse.Ok(content.Content, traceId))
                    {
                        StatusCode = content.StatusCode ?? StatusCodes.Status200OK
                    };
                    break;

                default:
                    break;
            }

            await next();
        }
    }
}
