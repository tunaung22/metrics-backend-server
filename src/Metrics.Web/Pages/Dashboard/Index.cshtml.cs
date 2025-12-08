using Metrics.Application.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metrics.Web.Pages.Dashboard;

[Authorize(Policy = ApplicationPolicies.CanAccess_AdminFeatures_Policy)]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
