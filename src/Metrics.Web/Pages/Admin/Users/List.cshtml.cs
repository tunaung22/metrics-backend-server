using Metrics.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Metrics.Web.Pages.Admin.Users
{
    public class ListModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ListModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public class UserListModel
        {
            public string Id { get; set; }
            public string UserName { get; set; }
            public string Email { get; set; }
        }

        public List<UserListModel> UserList;

        public async Task OnGet()
        {
            var users = _userManager.Users;
            UserList = await users.Select(u => new UserListModel
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email
            }).ToListAsync();
        }
    }
}
