using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace Metrics.Web.Pages.Manage.Users.Groups;

public class IndexModel : PageModel
{
    public readonly IUserTitleService _userTitleService;

    public IndexModel(IUserTitleService userTitleService)
    {
        _userTitleService = userTitleService;
    }

    public class UserTitleModel
    {
        public string? TitleName { get; set; }
    }

    public List<UserTitleModel> UserTitleList { get; set; } = [];

    public async Task<PageResult> OnGet()
    {
        var titles = await _userTitleService.FindAllAsync();
        UserTitleList = titles.Select(r => new UserTitleModel()
        {
            TitleName = r.TitleName ?? string.Empty
        }).ToList();

        return Page();
    }
}
