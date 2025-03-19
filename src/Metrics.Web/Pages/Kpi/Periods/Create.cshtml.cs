using Metrics.Application.DTOs.KpiPeriodDtos;
using Metrics.Application.Services.IServices;
using Metrics.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Metrics.Web.Pages.Kpi.Periods;

public class CreateModel : PageModel
{
    private readonly IKpiPeriodService _kpiPeriodService;

    public CreateModel(IKpiPeriodService kpiPeriodService)
    {
        _kpiPeriodService = kpiPeriodService;
        FormInput = new FormInputModel();
        ReturnUrl = string.Empty;
    }

    // ========== Binding Models ===============================================
    [BindProperty]
    public FormInputModel FormInput { get; set; }
    public string ReturnUrl { get; set; }

    public IActionResult OnGet()
    {
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
            var createDto = new KpiPeriodCreateDto
            {
                PeriodName = FormInput.PeriodName,
                SubmissionStartDate = FormInput.SubmissionStartDate,
                SubmissionEndDate = FormInput.SubmissionEndDate
            };
            await _kpiPeriodService.Create_Async(createDto);
            return RedirectToPage("Index");

        }
        catch (DuplicateContentException ex)
        {
            ModelState.AddModelError("DuplicateContent", "Duplicate KPI Period");
        }
        catch (Exception)
        {

            throw;
        }

        return Page();

    }

    public IActionResult OnPostCancel()
    {
        if (!string.IsNullOrEmpty(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }
        return RedirectToPage("./Index");
    }

    public class FormInputModel
    {
        [Required(ErrorMessage = "Period Name is required.")]
        public string PeriodName { get; set; } = null!;

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MMM/yyyy}")]
        [DataType(DataType.DateTime)]
        public DateTimeOffset SubmissionStartDate { get; set; } = DateTimeOffset.UtcNow.UtcDateTime;

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MMM/yyyy}")]
        [DataType(DataType.DateTime)]
        public DateTimeOffset SubmissionEndDate { get; set; } = DateTimeOffset.UtcNow.UtcDateTime;
    }

}
