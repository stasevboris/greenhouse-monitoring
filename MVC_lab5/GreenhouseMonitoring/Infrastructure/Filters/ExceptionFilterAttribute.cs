using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GreenhouseMonitoring.Infrastructure.Filters;


public class ExceptionFilterAttribute : Attribute, IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        string actionName = context.ActionDescriptor.DisplayName ?? "Неизвестное действие";
        string exceptionMessage = context.Exception.Message;
        string exceptionStack = context.Exception.StackTrace ?? "Стек вызовов недоступен";

        context.Result = new ContentResult
        {
            Content = $"В методе {actionName} возникло исключение:\n\n{exceptionMessage}\n\n{exceptionStack}",
            ContentType = "text/plain; charset=utf-8",
            StatusCode = 500
        };

        context.ExceptionHandled = true;
    }
}