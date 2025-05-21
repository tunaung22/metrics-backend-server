using Metrics.Application.Domains;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Metrics.Web.Pages.Account.Me;

public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(UserManager<ApplicationUser> userManager)
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

    public UserModel UserInfo { get; set; } = new();

    public async Task<PageResult> OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Page();
        }

        // var currentUser = await _userManager.FindByIdAsync(userId);
        var currentUser = await _userManager.Users
            .Include(u => u.Department)
            .Include(u => u.UserTitle)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (currentUser != null)
        {

            var roles = await _userManager.GetRolesAsync(currentUser);

            UserInfo = new UserModel
            {
                UserCode = currentUser?.UserCode,
                UserName = currentUser?.UserName,
                Email = currentUser?.Email,
                FullName = currentUser?.FullName,
                ContactAddress = currentUser?.ContactAddress,
                PhoneNumber = currentUser?.PhoneNumber,
                DepartmentName = currentUser?.Department?.DepartmentName,
                UserRoles = roles.ToList(),
                TitleName = currentUser?.UserTitle?.TitleName
            };
        }


        return Page();
    }
}
