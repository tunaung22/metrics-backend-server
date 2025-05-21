using Metrics.Application.Domains;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Metrics.Web.Pages.Manage.Submissions.Periods;


[Authorize(Policy = "CanAccessAdminFeaturePolicy")]
public class EditModel : PageModel
{
    private readonly IKpiSubmissionPeriodService _kpiPeriodService;

    public EditModel(IKpiSubmissionPeriodService kpiPeriodService)
    {
        _kpiPeriodService = kpiPeriodService;
    }

    // ========== MODELS ==========
    public class KpiPeriodFormInputModel
    {
        [Required(ErrorMessage = "Period Name is required.")]
        [StringLength(7, MinimumLength = 7, ErrorMessage = "Invalid name pattern. Period name must be 4 numbers + - + 2 numbers.")]
        [RegularExpression(@"^\d{4}-\d{2}$", ErrorMessage = "The date must be in the format YYYY-MM.")]
        public string PeriodName { get; set; } = null!;

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MMM/yyyy}")]
        [DataType(DataType.DateTime)]
        public DateTimeOffset SubmissionStartDate { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MMM/yyyy}")]
        [DataType(DataType.DateTime)]
        public DateTimeOffset SubmissionEndDate { get; set; }
    }

    [BindProperty]
    public KpiPeriodFormInputModel FormInput { get; set; } = new KpiPeriodFormInputModel();

    [BindProperty]
    public string PeriodName { get; set; } = string.Empty;

    public string ReturnUrl { get; set; } = string.Empty;

    // ========== HANDLERS ==========
    public async Task<PageResult> OnGetAsync(string periodName)
    {
        PeriodName = periodName;
        var result = await _kpiPeriodService.FindByKpiPeriodNameAsync(periodName);
        if (result == null)
            ModelState.AddModelError("FormInput.PeriodName", "Period not found.");
        else
        {
            FormInput = new KpiPeriodFormInputModel
            {
                PeriodName = result.PeriodName,
                SubmissionStartDate = result.SubmissionStartDate.ToLocalTime(),
                SubmissionEndDate = result.SubmissionEndDate.ToLocalTime()
            };
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            var entity = new KpiSubmissionPeriod
            {
                PeriodName = PeriodName,
                SubmissionStartDate = FormInput.SubmissionStartDate.UtcDateTime,
                SubmissionEndDate = FormInput.SubmissionEndDate.UtcDateTime
            };
            await _kpiPeriodService.UpdateAsync(PeriodName, entity);
        }
        catch (DuplicateContentException)
        {
            ModelState.AddModelError("FormInput.PeriodName", "Period name already exist.");
            return Page();
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "Unexpected error occured. Please try again.");
            return Page();
        }

        if (!string.IsNullOrEmpty(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }
        return RedirectToPage("./Index");
    }

    public IActionResult OnPostCancel()
    {
        if (!string.IsNullOrEmpty(ReturnUrl))
            return LocalRedirect(ReturnUrl);

        return RedirectToPage("./Index");
    }
}
