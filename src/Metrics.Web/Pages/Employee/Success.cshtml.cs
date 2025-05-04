using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metrics.Web.Pages.Employee;

public class SuccessModel : PageModel
{
    public string? Username { get; set; } = string.Empty;
    public string? FullName { get; set; } = string.Empty;
    public string? RoleName { get; set; } = string.Empty;

    public IActionResult OnGet()
    {
        Username = TempData["Username"] as string;
        FullName = TempData["FullName"] as string;
        RoleName = TempData["RoleName"] as string;

        return Page();
    }
}
