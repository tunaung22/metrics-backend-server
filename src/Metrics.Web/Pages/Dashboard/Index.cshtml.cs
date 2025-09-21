using Metrics.Application.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metrics.Web.Pages.Dashboard;

[Authorize(Policy = ApplicationPolicies.CanAccessAdminFeaturesPolicy)]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
