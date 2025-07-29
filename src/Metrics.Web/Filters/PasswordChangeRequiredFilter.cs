using Metrics.Application.Domains;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Metrics.Web.Filters;

public class PasswordChangeRequiredFilter : IAsyncPageFilter
{
    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var signInManager = context.HttpContext.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();
        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();

        if (signInManager.IsSignedIn(context.HttpContext.User))
        {
            var user = await userManager.GetUserAsync(context.HttpContext.User);
            if (user != null && user.IsPasswordChangeRequired)
            {
                // Skip redirection if already on the password change page
                if (!context.HttpContext.Request.Path.StartsWithSegments("/Account/ForceChangePassword"))
                {
                    context.Result = new RedirectToPageResult("/Account/ForceChangePassword");
                    return;
                }
            }
        }

        await next();
    }

    public async Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        await Task.CompletedTask;
    }
}
