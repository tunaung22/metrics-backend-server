using Metrics.Application.Domains;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metrics.Web.Pages.Manage.Submissions.KeyKpi.Keys;

public class EditModel : PageModel
{
    private readonly IKpiSubmissionPeriodService _kpiPeriodService;
    private readonly IKeyMetricService _keyMetricService;

    public EditModel(
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
    public required InputModel Input { get; set; } = new();

    [BindProperty]
    public string SelectedMetricCode { get; set; } = null!;
    [BindProperty]
    public string SelectedMetricTitle { get; set; } = null!;

    [BindProperty]
    public string? ReturnUrl { get; set; }


    public async Task<IActionResult> OnGetAsync(string code, string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("./Index");

        // find by code
        if (!string.IsNullOrEmpty(code) && Guid.TryParse(code, out Guid guidCode))
        {
            SelectedMetricCode = code;
            var keyMetric = await _keyMetricService.FindByCodeAsync(guidCode);
            if (keyMetric == null)
            {
                TempData["StatusMessage"] = "Key Metric not found.";
                // ModelState.AddModelError(string.Empty, "Not found");
                return RedirectToPage(ReturnUrl);
            }

            SelectedMetricTitle = keyMetric.MetricTitle;
            Input = new InputModel
            {
                MetricTitle = keyMetric.MetricTitle,
                Description = keyMetric.Description
            };
        }
        else
        {
            return RedirectToPage(ReturnUrl);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (SelectedMetricCode == null) return RedirectToPage("Index");
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var entity = new KeyMetric
            {
                MetricCode = Guid.Parse(SelectedMetricCode),
                MetricTitle = Input.MetricTitle.Trim(),
                Description = Input.Description?.Trim() ?? string.Empty
            };

            var updatedKey = await _keyMetricService.UpdateAsync(Guid.Parse(SelectedMetricCode), entity);
            TempData["StatusMessage"] = $"The key metric updated successfully.";

            if (!string.IsNullOrEmpty(ReturnUrl))
                return LocalRedirect(ReturnUrl);

            return RedirectToPage("Index");
        }
        catch (MetricsDuplicateContentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ModelState.AddModelError("Input.MetricTitle", ex.Message);
            return Page();
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Problem updating new metric.");
            return Page();
        }
    }

    public async Task<IActionResult> OnGetSearchTitleAsync(string value, string currentTitle)
    {
        var foundTitles = await _keyMetricService.SearchByMetricTitleAsync(value);

        if (foundTitles.Any())
        {
            var filteredCurrentTitle = foundTitles
                // .Where(k => !k.MetricTitle.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                // .Where(k => !k.MetricTitle.ToLower().Contains(value.ToLower()))
                .Where(k => !k.MetricTitle.Equals(currentTitle, StringComparison.InvariantCultureIgnoreCase))
                .ToList();
            return StatusCode(StatusCodes.Status200OK, new { data = filteredCurrentTitle });
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