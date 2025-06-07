using Metrics.Application.Domains;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace Metrics.Web.Pages.Manage.Submissions.KeyMetrics;

public class CreateModel : PageModel
{
    private readonly IKpiSubmissionPeriodService _kpiPeriodService;
    private readonly IKeyMetricService _keyMetricService;

    public CreateModel(
        IKpiSubmissionPeriodService kpiPeriodService,
        IKeyMetricService keyMetricService)
    {
        _kpiPeriodService = kpiPeriodService;
        _keyMetricService = keyMetricService;
    }

    public class InputModel
    {
        public string MetricTitle { get; set; } = null!;
        public string? Description { get; set; }
        public long KpiSubmissionPeriodId { get; set; }
    }

    [BindProperty]
    public required InputModel Input { get; set; }
    [BindProperty]
    public string SelectedMetricTitle { get; set; } = null!;
    public string? ReturnUrl { get; set; }


    public IActionResult OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("./Index");
        Input = new();
        // var periods = await _kpiPeriodService.FindAllAsync();
        // if (periods == null)
        // {
        //     ModelState.AddModelError(string.Empty, "No Period have added yet. Add Score Submission Periods and try again.");
        //     return Page();
        // }

        // KpiPeriodListItems = periods.Select(p => new SelectListItem
        // {
        //     Value = p.PeriodName,
        //     Text = $"{p.PeriodName} " +
        //         $"[ {p.SubmissionStartDate.ToLocalTime().ToString("MMM dd, yyyy")} " +
        //         $"to {p.SubmissionEndDate.ToLocalTime().ToString("MMM dd, yyyy")} ]"
        // }).ToList();

        // if (string.IsNullOrEmpty(Period))
        // {
        //     // Fetch Keys by Period
        // }

        return Page();
    }




    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page(); // Return the page with validation errors
        }

        try
        {
            var entity = new KeyMetric
            {
                MetricTitle = Input.MetricTitle.Trim(),
                Description = Input.Description?.Trim() ?? string.Empty
            };

            await _keyMetricService.CreateAsync(entity);

            TempData["StatusMessage"] = "A new key metric added successfully.";

            return RedirectToPage("Index");
        }
        catch (MetricsDuplicateContentException ex)
        {
            ModelState.AddModelError("Input.MetricTitle", ex.Message);
            return Page();
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Problem creating new metric.");
            return Page();
        }
    }

    public async Task<IActionResult> OnGetSearchTitleAsync(string value)
    {
        var foundTitles = await _keyMetricService.SearchByMetricTitleAsync(value);

        if (foundTitles.Any())
        {
            // return StatusCode(StatusCodes.Status409Conflict, new { message = "Period name already exist." });
            return StatusCode(StatusCodes.Status200OK, new { data = foundTitles });
        }

        return StatusCode(StatusCodes.Status200OK, new { data = Array.Empty<string>() });
    }

    public async Task<IActionResult> OnGetCheckDuplicateTitleAsync(string title)
    {
        var duplicateItem = await _keyMetricService.MetricTitleExistsAsync(title);
        if (duplicateItem)
        {
            return StatusCode(StatusCodes.Status409Conflict, new { message = "Key Metric Title already exist." });
        }

        return StatusCode(StatusCodes.Status200OK, new { message = "Key Metric Title is available." });
    }
}