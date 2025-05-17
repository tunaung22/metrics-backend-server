using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metrics.Web.Pages.Dashboard;

[Authorize(Policy = "RequiresAdminRole")]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
