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
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? UserCode { get; set; }
        public string? FullName { get; set; }
        public string? ContactAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public string? DepartmentName { get; set; }
        public List<string> UserRoles { get; set; } = [];
        public string? TitleName { get; set; }
    }

    public UserModel? UserInfo { get; set; }

    [BindProperty]
    public string? TargetUserId { get; set; }

    public string? ReturnUrl { get; set; } = string.Empty;


    public async Task<PageResult> OnGetAsync(string userId, string? returnUrl)
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
                    TempData["Message"] = "Cannot lock this user. Reason: lockout not enabled.";
                    return Page();
                }

                var roles = await _userManager.GetRolesAsync(targetUser);
                TargetUserId = targetUser?.UserName;

                UserInfo = new UserModel
                {
                    UserCode = targetUser?.UserCode,
                    UserName = targetUser?.UserName,
                    Email = targetUser?.Email,
                    FullName = targetUser?.FullName,
                    ContactAddress = targetUser?.ContactAddress,
                    PhoneNumber = targetUser?.PhoneNumber,
                    DepartmentName = targetUser?.Department?.DepartmentName,
                    UserRoles = roles.ToList(),
                    TitleName = targetUser?.UserTitle?.TitleName
                };
            }
            else
                TempData["Message"] = "User does not exist.";
        }
        else
            TempData["Message"] = "User does not exist.";

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
            .FirstOrDefaultAsync(u => u.UserName == TargetUserId);

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
                        TempData["StatusMessage"] = $"The account <strong>{TargetUserId}</strong> has been locked.";
                    else
                        TempData["StatusMessage"] = "Error: Failed to lock out the user.";
                }
                else
                {
                    var result = await _userManager
                          .SetLockoutEndDateAsync(targetUser, DateTimeOffset.UtcNow.AddYears(100));
                    if (result.Succeeded)
                        TempData["StatusMessage"] = $"The account <strong>{TargetUserId}</strong> has been unlocked.";
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
