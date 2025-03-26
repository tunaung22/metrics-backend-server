using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Metrics.Application.DTOs.DepartmentDtos;
using Metrics.Application.Services.IServices;
using Metrics.Domain.Entities;
using Metrics.Web.Mappers.ViewModelMappers;
using Metrics.Web.Models.DepartmentViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

namespace Metrics.Web.Pages.Departments;

public class IndexModel : PageModel
{
    private readonly IDepartmentService _departmentService;

    public IndexModel(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }


    // =============== Models ==========================================
    [BindProperty]
    public IEnumerable<DepartmentModel> Departments { get; set; } = [];


    // =============== Handlers ==========================================
    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var result = await _departmentService.FindAll_Async();
            if (result.Count() > 0)
            {
                Departments = result.Select(e => new DepartmentModel
                {
                    DepartmentCode = e.DepartmentCode,
                    DepartmentName = e.DepartmentName
                }).ToList();
            }

            return Page();
        }
        catch (Exception e)
        {
            ModelState.AddModelError("DepartmentGetAll", e.Message);
            throw;
        }
    }

    public class DepartmentModel
    {

        [Required]
        // [StringLength(100)]
        public Guid DepartmentCode { get; set; }

        [Required]
        [StringLength(200)]
        public string DepartmentName { get; set; } = null!;

        // UI-specific properties
        // public bool IsSelected { get; set; }
        // ...
    }

}
