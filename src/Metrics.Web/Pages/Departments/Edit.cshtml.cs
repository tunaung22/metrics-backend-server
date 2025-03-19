using Metrics.Application.DTOs.DepartmentDtos;
using Metrics.Application.Services.IServices;
using Metrics.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace Metrics.Web.Pages.Departments;

public class EditModel : PageModel
{

    private readonly IDepartmentService _departmentService;

    public EditModel(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }



    [BindProperty]
    public InputModel Input { get; set; }

    [BindProperty]
    public string DepartmentCode { get; set; }

    public string ReturnUrl { get; set; }

    public async Task<IActionResult> OnGet(string departmentCode)
    {
        DepartmentCode = departmentCode;
        var result = await _departmentService.FindByDepartmentCode_Async(departmentCode);
        if (result == null)
        {
            ModelState.AddModelError("Input.DepartmentName", "Department not found.");
        }
        else
        {
            // dto to inputmodel
            Input = new InputModel
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

        var updateDto = new DepartmentUpdateDto
        {
            DepartmentName = Input.DepartmentName
        };
        await _departmentService.Update_Async(DepartmentCode, updateDto);

        // Redirect to return URL or default to Index
        if (!string.IsNullOrEmpty(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }
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

    public class InputModel
    {
        [Required(ErrorMessage = "Department name is needed.")]
        [StringLength(200)]
        public string DepartmentName { get; set; } = null!;
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
