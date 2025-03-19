using Metrics.Application.DTOs.DepartmentDtos;
using Metrics.Application.Services.IServices;
using Metrics.Domain.Entities;
using Metrics.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Metrics.Web.Pages.Departments
{
    public class CreateModel : PageModel
    {
        private readonly IDepartmentService _departmentService;

        public CreateModel(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
            Input = new InputModel();
            ReturnUrl = string.Empty;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IActionResult OnGet(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl))
                ReturnUrl = returnUrl;

            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var createDto = new DepartmentCreateDto()
                {
                    DepartmentName = Input.DepartmentName
                };

                var created = await _departmentService.Create_Async(createDto);

            }
            catch (DuplicateContentException ex)
            {
                ModelState.AddModelError("Input.DepartmentName", ex.Message);
                return Page();
            }
            catch (Exception ex)
            {

                // ModelState.AddModelError("Input.DepartmentName", "Duplicate department.");
                ModelState.AddModelError("Input.DepartmentName", "Failed to create department.");
                return Page();
            }

            if (!string.IsNullOrEmpty(ReturnUrl))
                return LocalRedirect(ReturnUrl);

            return RedirectToPage("./Index");
        }

        public class InputModel()
        {
            [Required(ErrorMessage = "Department name is needed.")]
            [StringLength(200)]
            public string DepartmentName { get; set; } = null!;
        }
    }


    // public async Task<IActionResult> OnPostAsync()
    //     {
    //         if (!ModelState.IsValid)
    //         {
    //             // var query = await _departmentService.GetAllAsync();
    //             // Departments = query.ToViewModel();
    //             await LoadDepartments();

    //             // if (DepartmentCreateModel == null)
    //             //     DepartmentCreateModel = new DepartmentCreateViewModel();

    //             return Page();
    //         }

    //         //_context.Items.Add(item);
    //         //await _context.SaveChangesAsync();
    //         //return RedirectToPage();
    //         try
    //         {
    //             // var entity = DepartmentCreateModel.ToEntity();
    //             var entity = new Department
    //             {
    //                 DepartmentName = Input.DepartmentName
    //             };
    //             var createdEntity = await _departmentService.CreateAsync(entity);

    //             // if (!EditMode)
    //             // {
    //             //     // var updateDto = FormData.ToUpdateDto();
    //             //     // await _departmentService.UpdateAsync(updateDto);
    //             //     var createdEntity = await _departmentService.CreateAsync(entity);
    //             // }
    //             // else
    //             // {
    //             //     // var createDto = FormData.ToCreateDto();
    //             //     // await _departmentService.CreateAsync(createDto);
    //             //     var updatedEntity = await _departmentService.UpdateAsync(entity);
    //             // }
    //         }
    //         catch (InvalidOperationException ex)
    //         {
    //             await LoadDepartments();
    //             ModelState.AddModelError("DepartmentName", ex.Message);

    //             return Page();
    //         }
    //         catch (Exception ex)
    //         {
    //             ModelState.AddModelError("DepartmentName", ex.Message);
    //             throw;
    //         }


    //         return RedirectToPage();
    //     }



    //  public async Task<IActionResult> OnPostEdit(string departmentCode)
    //     {
    //         EditMode = true;
    //         ModelState.Clear();
    //         try
    //         {
    //             // Find the product to edit
    //             // var productToEdit = await _departmentService.GetByDepartmentCodeAsync(departmentCode);
    //             // FormData = productToEdit?.ToFormViewModel();
    //             var department = await _departmentService.FindByDepartmentCodeAsync(departmentCode);
    //             if (!department.Success)
    //             {
    //                 if (department.ErrorType == Application.Results.ErrorType.NotFound)
    //                     // not found!!
    //                     ModelState.AddModelError("DepartmentEdit", "Selected department not found.");
    //                 return Page();
    //             }
    //             Input = new InputModel
    //             {
    //                 DepartmentName = department.Data.DepartmentName
    //             };
    //             DepartmentUpdateModel = department.Data.ToUpdateViewModel();

    //             return Page();
    //         }
    //         catch (Exception ex)
    //         {
    //             ModelState.AddModelError("DepartmentName", ex.Message);
    //             return Page();
    //         }
    //     }
}
