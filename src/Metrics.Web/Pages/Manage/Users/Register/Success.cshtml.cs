using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Metrics.Web.Pages.Manage.Users.Register;

public class SuccessModel : PageModel
{
    public string? Username { get; set; } = string.Empty;
    public string? FullName { get; set; } = string.Empty;
    public string? GroupName { get; set; } = string.Empty;
    public List<string> AssignedRoles { get; set; } = [];

    public IActionResult OnGet()
    {
        Username = TempData["Username"] as string;
        FullName = TempData["FullName"] as string;
        GroupName = TempData["GroupName"] as string;
        var rolesJson = TempData["AssignedRoles"] as string;
        AssignedRoles = (rolesJson != null ? JsonSerializer.Deserialize<List<string>>(rolesJson) : new List<string>()) ?? [];

        if (string.IsNullOrEmpty(Username))
            return RedirectToPage("./Index");

        return Page();
    }
}
