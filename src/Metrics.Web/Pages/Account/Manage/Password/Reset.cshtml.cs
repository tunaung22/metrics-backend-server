using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Metrics.Web.Pages.Account.Manage.Password;

public class ResetModel : PageModel
{
    // =============== MODELS ==================================================
    public class InputModel
    {
        [Required]
        [Display(Name = "Username")]
        public string? UserName { get; set; }

        [Required]
        [Display(Name = "User Code")]
        public string? UserCode { get; set; }
    }

    [BindProperty]
    public required InputModel Input { get; set; }

    // =============== HANDLERS ================================================
    public IActionResult OnGet()
    {
        // TODO: Not implemented
        return RedirectToPage("/Index");
    }

    public IActionResult OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // 1. set new random password
        // 2. send new password string to sysadmin
        // 3. redirect to success page

        return Page();
    }

    // ========== Methods ==================================================
}
