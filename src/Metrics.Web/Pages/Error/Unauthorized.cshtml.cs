using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metrics.Web.Pages.Error;

public class UnauthorizedModel : PageModel
{
    // public string ErrorMessage { get; set; }


    public void OnGet(string returnUrl, string message)
    {
        // ErrorMessage = message ?? "You do not have permission to access this resource.";

        // if (string.IsNullOrEmpty(message) && !string.IsNullOrEmpty(returnUrl))
        // {
        //     // Map specific pages to custom messages
        //     if (returnUrl.Contains("/Admin/"))
        //         ErrorMessage = "Admin access required for this page.";
        //     else if (returnUrl.Contains("/Reports/"))
        //         ErrorMessage = "You need Report Manager privileges to view this content.";
        // }
    }
}
