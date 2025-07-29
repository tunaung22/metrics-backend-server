using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metrics.Web.Pages.Manage;

[Authorize(Policy = "CanAccessAdminFeaturePolicy")]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
