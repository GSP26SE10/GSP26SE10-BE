using Microsoft.AspNetCore.Mvc.Filters;

namespace BookfetSystem.API.Filters;

public sealed class NormalizePaginationActionFilter : IActionFilter
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 10;

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ActionArguments.TryGetValue("page", out var pageArg) && pageArg is int page && page <= 0)
        {
            context.ActionArguments["page"] = DefaultPage;
        }

        if (context.ActionArguments.TryGetValue("pageSize", out var pageSizeArg) && pageSizeArg is int pageSize && pageSize <= 0)
        {
            context.ActionArguments["pageSize"] = DefaultPageSize;
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
