using Metrics.Application.Domains;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Metrics.Web.Pages.Manage.Users;

public class ResetModel : PageModel
{
    private readonly ILogger<ResetModel> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    public ResetModel(
        ILogger<ResetModel> logger,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _logger = logger;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // =============== MODELS ==================================================
    public class InputModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; } = null!;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }
    }
    // =========================================================================
    [BindProperty]
    public required InputModel Input { get; set; }

    [BindProperty]
    public string TargetUserId { get; set; } = string.Empty;

    public string TargetUserName { get; set; } = null!;

    [TempData]
    public string? StatusMessage { get; set; }

    public string ReturnUrl { get; set; } = string.Empty;

    // =============== HANDLERS ================================================

    public async Task<IActionResult> OnGetAsync(string userId, string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl))
            ReturnUrl = returnUrl;

        if (string.IsNullOrEmpty(userId))
        {
            TempData["StatusMessage"] = "User does not exist.";
            return RedirectToPage("Index");
        }

        TargetUserId = userId;

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            TempData["StatusMessage"] = "User does not exist.";
            return RedirectToPage("Index");
        }

        TargetUserName = user.UserName!;

        var hasPassword = await _userManager.HasPasswordAsync(user);
        if (!hasPassword)
            // return RedirectToPage("./SetPassword");
            ModelState.AddModelError(string.Empty, "User has no password set.");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByIdAsync(TargetUserId);
        if (user != null)
        {

            var removePasswordResult = await _userManager.RemovePasswordAsync(user);
            if (!removePasswordResult.Succeeded)
            {
                foreach (var error in removePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            var addPasswordResult = await _userManager.AddPasswordAsync(user, Input.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                foreach (var error in addPasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("Admin reset user password successfully.");
            TempData["StatusMessage"] = $"Password for user {user.UserName} has been changed.";
        }

        return RedirectToPage("./Index");
    }

    public IActionResult OnPostCancel()
    {
        var returnUrl = ViewData["ReturnUrl"] as string;
        if (!string.IsNullOrEmpty(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToPage("Index");
    }

    // ========== Methods ==================================================

}
