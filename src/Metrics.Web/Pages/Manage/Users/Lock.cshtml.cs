using Metrics.Application.Domains;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Metrics.Web.Pages.Manage.Users;

public class LockModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public LockModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public class UserModel
    {
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string UserCode { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? ContactAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public string? DepartmentName { get; set; }
        public List<string> UserRoles { get; set; } = [];
        public string? TitleName { get; set; }
    }

    public UserModel UserInfo { get; set; } = new UserModel();

    [BindProperty]
    public string? TargetUserId { get; set; }
    public bool IsLocked { get; set; } // Lock Status

    public string? ReturnUrl { get; set; } = string.Empty;


    public async Task<IActionResult> OnGetAsync(string userId, string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl))
            ReturnUrl = returnUrl;

        if (userId != null)
        {
            // var currentUser = await _userManager.FindByIdAsync(userId);
            var targetUser = await _userManager.Users
                .Include(u => u.Department)
                .Include(u => u.UserTitle)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (targetUser != null)
            {
                if (!targetUser.LockoutEnabled)
                {
                    TempData["StatusMessage"] = "Cannot lock this user. Reason: lockout not enabled.";
                    // return Page();
                    return RedirectToPage("Index");
                }
                else
                {
                    if (targetUser.LockoutEnd > DateTimeOffset.UtcNow) // if locked
                        IsLocked = true;
                    else
                        IsLocked = false;
                }

                var roles = await _userManager.GetRolesAsync(targetUser);
                TargetUserId = targetUser.Id;

                UserInfo = new UserModel
                {
                    UserCode = targetUser.UserCode,
                    UserName = targetUser.UserName ?? string.Empty,
                    Email = targetUser.Email ?? string.Empty,
                    FullName = targetUser.FullName,
                    ContactAddress = targetUser?.ContactAddress,
                    PhoneNumber = targetUser?.PhoneNumber,
                    DepartmentName = targetUser?.Department?.DepartmentName,
                    UserRoles = roles.ToList(),
                    TitleName = targetUser?.UserTitle?.TitleName
                };
            }
            else
            {
                TempData["StatusMessage"] = "User does not exist.";
                return RedirectToPage("Index");
            }
        }
        else
        {
            TempData["StatusMessage"] = "User does not exist.";
            return RedirectToPage("Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(TargetUserId))
            ModelState.AddModelError(string.Empty, "User not found.");

        // var isLocked = _userManager.IsLockedOutAsync(TargetUser);
        var targetUser = await _userManager.Users
            .Include(u => u.Department)
            .Include(u => u.UserTitle)
            .FirstOrDefaultAsync(u => u.Id == TargetUserId);

        if (targetUser != null)
        {
            if (!targetUser.LockoutEnabled)
            {
                TempData["Message"] = "Cannot lock this user. Reason: lockout not enabled.";
            }
            else
            {
                // Toggle Lock
                if (targetUser.LockoutEnd > DateTimeOffset.UtcNow) // if locked
                {
                    var result = await _userManager
                        .SetLockoutEndDateAsync(targetUser, DateTimeOffset.UtcNow);
                    if (result.Succeeded)
                        TempData["StatusMessage"] = $"The account <strong>{targetUser.UserName}</strong> has been unlocked.";
                    else
                        TempData["StatusMessage"] = "Error: Failed to lock out the user.";
                }
                else
                {
                    var result = await _userManager
                          .SetLockoutEndDateAsync(targetUser, DateTimeOffset.UtcNow.AddYears(100));
                    if (result.Succeeded)
                        TempData["StatusMessage"] = $"The account <strong>{targetUser.UserName}</strong> has been locked.";
                    else
                        TempData["StatusMessage"] = "Error: Failed to unlock the user.";
                }


                return RedirectToPage("Index");
            }
        }

        return Page();
    }

    public IActionResult OnPostCancel()
    {
        if (!string.IsNullOrEmpty(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }
        return RedirectToPage("Index");
    }
}
