using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metrics.Web.Pages.Manage.Submissions.KeyMetrics;

[Authorize(Policy = "CanAccessAdminFeaturePolicy")]
public class IndexModel : PageModel
{
    private readonly IKeyMetricService _keyMetricService;


    public IndexModel(IKeyMetricService keyMetricService)
    {
        _keyMetricService = keyMetricService;
    }


    // ========== MODELS =======================================================
    public class KeyMetricViewModel
    {
        public string? MetricCode { get; set; }
        public string? MetricTitle { get; set; }
        public string? Description { get; set; }
    }

    public List<KeyMetricViewModel> KeyMetrics { get; set; } = [];

    // ============ Pagination =================================================
    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public long TotalItems { get; set; } // Count
    public int TotalPages => (int)Math.Ceiling(decimal.Divide(TotalItems, PageSize));
    public bool ShowPrevious => CurrentPage > 1;
    public bool ShowNext => CurrentPage < TotalPages;
    // Return URL
    public string? ReturnUrl { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }


    // ========== HANDLERS =====================================================
    public async Task<IActionResult> OnGetAsync(
        string? returnUrl,
        int currentPage,
        int pageSize)
    {
        // ReturnUrl = returnUrl ??= "Index";
        CurrentPage = currentPage == 0 ? 1 : currentPage;
        PageSize = pageSize == 0 ? 20 : pageSize;
        KeyMetrics = await LoadKeyMetrics(CurrentPage, PageSize);

        return Page();
    }

    public async Task<IActionResult> GetKeyMetricAsync(string code)
    {
        if (string.IsNullOrEmpty(code) || !Guid.TryParse(code, out var guidCode))
        {
            return BadRequest("Invalid Key Metric Code.");
        }

        var keyMetrics = await _keyMetricService.FindByCodeAsync(guidCode);

        if (keyMetrics == null)
        {
            return NotFound("Key Metric not found.");
        }

        return new JsonResult(new { Title = keyMetrics.MetricTitle, Description = keyMetrics.Description });
    }

    public async Task<IActionResult> OnGetEditAsync(string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            ModelState.AddModelError(string.Empty, "Key Metric not found.");
        }

        var keyMetrics = await _keyMetricService.FindByCodeAsync(new Guid(code));

        if (keyMetrics == null)
        {
            ModelState.AddModelError(string.Empty, "Key Metric not found.");
            return Page();
        }

        return Page();
    }

    private async Task<List<KeyMetricViewModel>> LoadKeyMetrics(int currentPage, int pageSize)
    {
        var periods = await _keyMetricService.FindAllAsync(
            pageNumber: currentPage,
            pageSize: pageSize);
        if (periods.Any())
        {
            return periods.Select(k => new KeyMetricViewModel()
            {
                MetricCode = k.MetricCode.ToString(),
                MetricTitle = k.MetricTitle,
                Description = k.Description

            }).ToList();
        }

        return [];
    }

}
