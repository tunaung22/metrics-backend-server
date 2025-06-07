using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Metrics.Web.Pages.Manage.Submissions.DepartmentKeyMetrics;


public class CreateModel : PageModel
{
    private readonly IKpiSubmissionPeriodService _kpiPeriodService;

    public CreateModel(
        IKpiSubmissionPeriodService kpiPeriodService
        )
    {
        _kpiPeriodService = kpiPeriodService;
    }


    public class InputModel
    {
        public string? MetricTitle { get; set; }
        public string? Description { get; set; }
        public long KpiSubmissionPeriodId { get; set; }
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty]
    public List<SelectListItem> KpiPeriodListItems { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? Period { get; set; }

    public string? ReturnUrl { get; set; }


    public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("./Index");

        var periods = await _kpiPeriodService.FindAllAsync();
        if (periods == null)
        {
            ModelState.AddModelError(string.Empty, "No Period have added yet. Add Score Submission Periods and try again.");
            return Page();
        }

        KpiPeriodListItems = periods.Select(p => new SelectListItem
        {
            Value = p.PeriodName,
            Text = $"{p.PeriodName} " +
                $"[ {p.SubmissionStartDate.ToLocalTime().ToString("MMM dd, yyyy")} " +
                $"to {p.SubmissionEndDate.ToLocalTime().ToString("MMM dd, yyyy")} ]"
        }).ToList();

        if (string.IsNullOrEmpty(Period))
        {
            // Fetch Keys by Period
        }

        return Page();
    }


    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page(); // Return the page with validation errors
        }

        // Handle valid form submission (e.g., save data, redirect, etc.)
        return RedirectToPage(); // Redirect or perform other actions
    }
}