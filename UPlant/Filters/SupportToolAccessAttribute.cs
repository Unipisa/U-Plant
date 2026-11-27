using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace UPlant.Filters;

[AttributeUsage(AttributeTargets.Class)]
public sealed class SupportToolAccessAttribute : ActionFilterAttribute
{
    private readonly string _moduleRole;
    private readonly bool _operatorCanReadWithoutModule;
    private readonly HashSet<string> _readActions;

    public SupportToolAccessAttribute(string moduleRole, bool operatorCanReadWithoutModule, params string[] readActions)
    {
        _moduleRole = moduleRole;
        _operatorCanReadWithoutModule = operatorCanReadWithoutModule;
        _readActions = new HashSet<string>(readActions, StringComparer.OrdinalIgnoreCase);
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.User;
        if (user.IsInRole("Administrator")) return;

        var isRead = _readActions.Contains(context.ActionDescriptor.RouteValues["action"] ?? string.Empty);
        var allowed = user.IsInRole("Operator")
            ? user.IsInRole(_moduleRole) || (isRead && _operatorCanReadWithoutModule)
            : user.IsInRole("Reader") && user.IsInRole(_moduleRole) && isRead;

        if (!allowed) context.Result = new ForbidResult();
    }
}
