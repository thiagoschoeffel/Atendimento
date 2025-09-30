using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Atendimento.Api.Filters
{
    public class FluentValidationActionFilter : IAsyncActionFilter, IOrderedFilter
    {
        public int Order => 0;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var provider = context.HttpContext.RequestServices;

            foreach (var (name, arg) in context.ActionArguments)
            {
                if (arg is null) continue;

                var validatorType = typeof(IValidator<>).MakeGenericType(arg.GetType());
                var validator = provider.GetService(validatorType) as IValidator;
                if (validator is null) continue;

                var result = await validator.ValidateAsync(new ValidationContext<object>(arg), context.HttpContext.RequestAborted);
                if (!result.IsValid)
                {
                    foreach (var e in result.Errors)
                        context.ModelState.AddModelError(e.PropertyName, e.ErrorMessage);
                }
            }

            await next();
        }
    }
}
