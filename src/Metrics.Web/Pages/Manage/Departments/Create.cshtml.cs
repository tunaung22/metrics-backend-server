using Metrics.Application.Domains;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Metrics.Web.Pages.Manage.Departments;

[Authorize(Policy = "CanAccessAdminFeaturePolicy")]
public class CreateModel : PageModel
{
    private readonly IDepartmentService _departmentService;

    public CreateModel(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
        Input = new InputModel();
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public string? ReturnUrl { get; set; } = string.Empty;

    public IActionResult OnGet(string? returnUrl)
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
            var entity = new Department
            {
                DepartmentName = Input.DepartmentName
            };

            var created = await _departmentService.CreateAsync(entity);

        }
        catch (DuplicateContentException ex)
        {
            ModelState.AddModelError("Input.DepartmentName", ex.Message);
            return Page();
        }
        catch (Exception)
        {
            ModelState.AddModelError("Input.DepartmentName", "Failed to create department.");
            return Page();
        }

        if (!string.IsNullOrEmpty(ReturnUrl))
            return LocalRedirect(ReturnUrl);

        return RedirectToPage("Index");
    }

    public IActionResult OnPostCancel()
    {
        if (!string.IsNullOrEmpty(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }
        return RedirectToPage("Index");
    }

    public class InputModel()
    {
        [Required(ErrorMessage = "Department name is needed.")]
        [StringLength(200)]
        public string DepartmentName { get; set; } = null!;
    }
}
