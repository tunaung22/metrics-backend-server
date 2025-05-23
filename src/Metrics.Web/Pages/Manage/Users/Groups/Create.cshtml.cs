using Metrics.Application.Domains;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metrics.Web.Pages.Manage.Users.Groups;

public class CreateModel : PageModel
{
    private readonly IUserTitleService _userTitleService;

    public CreateModel(IUserTitleService userTitleService)
    {
        _userTitleService = userTitleService;
    }

    // =============== MODELS ==================================================
    public class InputModel
    {
        public string TitleName { get; set; } = null!;
        public string? Description { get; set; }
    }

    [BindProperty]
    public required InputModel Input { get; set; }

    public string? ReturnUrl { get; set; } = string.Empty;

    public PageResult OnGet(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl))
            ReturnUrl = returnUrl;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var entity = new UserTitle
            {
                TitleName = Input.TitleName,
                Description = Input.Description ?? string.Empty
            };

            var created = await _userTitleService.CreateAsync(entity);

        }
        catch (DuplicateContentException ex)
        {
            ModelState.AddModelError("Input.TitleName", ex.Message);
            return Page();
        }
        catch (Exception)
        {
            ModelState.AddModelError("Input.TitleName", "Failed to create user title.");
            return Page();
        }

        if (!string.IsNullOrEmpty(ReturnUrl))
            return LocalRedirect(ReturnUrl);

        return RedirectToPage("./Index");
    }

    public IActionResult OnPostCancel()
    {
        if (!string.IsNullOrEmpty(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }
        return RedirectToPage("./Index");
    }
}
