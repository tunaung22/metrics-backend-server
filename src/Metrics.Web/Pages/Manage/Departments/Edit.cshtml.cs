using Metrics.Application.Domains;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Metrics.Web.Pages.Manage.Departments;

public class EditModel : PageModel
{

    private readonly IDepartmentService _departmentService;

    public EditModel(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }


    // ========== MODELS ==========
    public class DepartmentInputModel
    {
        [Required(ErrorMessage = "Department name is needed.")]
        [StringLength(200)]
        public string DepartmentName { get; set; } = null!;
    }

    [BindProperty]
    public DepartmentInputModel FormInput { get; set; } = new DepartmentInputModel();

    [BindProperty]
    public string DepartmentCode { get; set; } = string.Empty;

    public string ReturnUrl { get; set; } = string.Empty;


    // ========== HANDLERS ========================================
    public async Task<IActionResult> OnGetAsync(string departmentCode, string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl))
            ReturnUrl = returnUrl;

        DepartmentCode = departmentCode;
        var result = await _departmentService.FindByDepartmentCodeAsync(departmentCode);
        if (result == null)
        {
            ModelState.AddModelError("FormInput.DepartmentName", "Department not found.");
        }
        else
        {
            // dto to inputmodel
            FormInput = new DepartmentInputModel
            {
                DepartmentName = result.DepartmentName
            };
        }

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
            // var updateDto = new DepartmentUpdateDto
            // {
            //     DepartmentName = Input.DepartmentName
            // };
            var entity = new Department
            {
                DepartmentCode = Guid.Parse(DepartmentCode),
                DepartmentName = FormInput.DepartmentName
            };
            await _departmentService.UpdateAsync(DepartmentCode, entity);
        }
        catch (DuplicateContentException)
        {
            ModelState.AddModelError("FormInput.DepartmentName", "Department already exist.");
            return Page();
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "Unexpected error occured. Please try again.");
            return Page();
        }


        // Redirect to return URL or default to Index
        var returnUrl = ViewData["ReturnUrl"] as string;
        if (!string.IsNullOrEmpty(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToPage("./Index");
    }

    public IActionResult OnPostCancel()
    {
        if (!string.IsNullOrEmpty(ReturnUrl))
            return LocalRedirect(ReturnUrl);

        return RedirectToPage("./Index");
    }


}
// public async Task<IActionResult> OnPostUpdate()
// {
//     EditMode = true;

//     if (!ModelState.IsValid)
//     {
//         if (DepartmentUpdateModel == null)
//             DepartmentUpdateModel = new DepartmentUpdateViewModel();


//         return Page();
//     }

//     try
//     {
//         if (EditMode)
//         {
//             // var updateDto = FormData.ToUpdateDto();
//             // await _departmentService.UpdateAsync(updateDto);
//             var entity = DepartmentUpdateModel.ToEntity();
//             await _departmentService.UpdateAsync(entity);
//         }
//     }
//     catch (InvalidOperationException ex)
//     {
//         ModelState.AddModelError("DepartmentName", ex.Message);

//         return Page();
//     }
//     catch (Exception ex)
//     {
//         ModelState.AddModelError("DepartmentName", ex.Message);
//         throw;
//     }
//     finally
//     {
//         await LoadDepartments();
//     }


//     return RedirectToPage();

// }
