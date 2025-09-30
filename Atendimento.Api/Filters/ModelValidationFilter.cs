using Atendimento.Api.Models.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Atendimento.Api.Filters
{
    public class ModelValidationFilter : IActionFilter, IOrderedFilter
    {
        public int Order => 10;

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(kv => kv.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kv => kv.Key,
                        kv => kv.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                var traceId = context.HttpContext.TraceIdentifier;
                var resp = ApiResponse.Fail("validation_error", "Dados inválidos.", errors, traceId);

                context.Result = new BadRequestObjectResult(resp);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
