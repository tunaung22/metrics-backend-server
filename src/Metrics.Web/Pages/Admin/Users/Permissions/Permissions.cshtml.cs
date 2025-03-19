using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metrics.Web.Pages.Admin.Users.Permissions;

public class PermissionsModel : PageModel
{
    [FromRoute]
    public string UserUUID { get; set; }

    public void OnGet()
    {
    }
}
