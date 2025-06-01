using Metrics.Application.Domains;
using Metrics.Application.DTOs.AccountDtos;
using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Metrics.Web.Pages.Account.Me;

public class EditModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IUserService _userService;
    private readonly IDepartmentService _departmentService;
    private readonly IUserTitleService _userTitleService;

    public EditModel(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IUserService userService,
        IDepartmentService departmentService,
        IUserTitleService userTitleService
        )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _userService = userService;
        _departmentService = departmentService;
        _userTitleService = userTitleService;
    }


    // ========== MODELS ==================================================
    public class InputModel
    {
        public string UserCode { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? ContactAddress { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } = string.Empty;
    }

    // ======================================================================
    [BindProperty]
    public required InputModel Input { get; set; } // form

    public bool IsAdmin { get; set; }

    public string? ReturnUrl { get; set; } // return url

    // ========== HANDLERS ==================================================
    public async Task<IActionResult> OnGetAsync(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl))
            ReturnUrl = returnUrl;

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        var roles = await _userManager.GetRolesAsync(currentUser);

        IsAdmin = roles.Contains("Admin");

        Input = new InputModel
        {
            UserCode = currentUser.UserCode,
            Email = currentUser.Email ?? string.Empty,
            FullName = currentUser.FullName,
            ContactAddress = currentUser.ContactAddress,
            PhoneNumber = currentUser.PhoneNumber
        };



        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            var roles = await _userManager.GetRolesAsync(currentUser);
            IsAdmin = roles.Contains("Admin");

            var updateDto = new UserProfileUpdateDto
            {
                UserCode = IsAdmin ? Input.UserCode : currentUser.UserCode,
                Email = Input.Email,
                FullName = Input.FullName,
                ContactAddress = Input.ContactAddress,
                PhoneNumber = Input.PhoneNumber
            };
            var updatedUser = await _userService.UpdateProfileAsync(currentUser.Id, updateDto);

            var returnUrl = ViewData["ReturnUrl"] as string;
            if (!string.IsNullOrEmpty(returnUrl))
                return LocalRedirect(returnUrl);

            TempData["StatusMessage"] = $"User profile updated successfully.";

            return RedirectToPage("/Account/Me/Index");
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Update user failed.");
        }

        return Page();
    }

    public IActionResult OnPostCancel()
    {
        if (!string.IsNullOrEmpty(ReturnUrl))
            return LocalRedirect(ReturnUrl);

        return RedirectToPage("Index");
    }

    // ========== METHODS ==================================================

}
