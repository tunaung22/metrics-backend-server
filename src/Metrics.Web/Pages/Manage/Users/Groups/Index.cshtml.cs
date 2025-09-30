using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metrics.Web.Pages.Manage.Users.Groups;

public class IndexModel(IUserTitleService userTitleService) : PageModel
{
    public readonly IUserTitleService _userTitleService = userTitleService;

    public class UserTitleModel
    {
        public string? TitleName { get; set; }
    }

    public List<UserTitleModel> UserTitleList { get; set; } = [];

    public async Task<PageResult> OnGet()
    {
        var titles = await _userTitleService.FindAllAsync();
        UserTitleList = titles
            .Where(g => !g.TitleName.Equals("sysadmin", StringComparison.OrdinalIgnoreCase))
            .Select(g => new UserTitleModel()
            {
                TitleName = g.TitleName ?? string.Empty
            }).ToList();

        return Page();
    }
}
