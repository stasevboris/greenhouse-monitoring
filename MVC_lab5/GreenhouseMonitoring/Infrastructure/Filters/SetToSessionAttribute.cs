// Infrastructure/Filters/SetToSessionAttribute.cs
using GreenhouseMonitoring.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GreenhouseMonitoring.Infrastructure.Filters;

public class SetToSessionAttribute(string name) : Attribute, IAsyncActionFilter
{
    private readonly string _name = name;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resultContext = await next();

        if (resultContext.Result is RedirectToActionResult or RedirectToRouteResult)
        {
            var filterObject = context.ActionArguments.Values.FirstOrDefault();
            if (filterObject != null)
            {
                try
                {
                    resultContext.HttpContext.Session.Set(_name, filterObject);
                }
                catch { }
            }
        }
    }
}