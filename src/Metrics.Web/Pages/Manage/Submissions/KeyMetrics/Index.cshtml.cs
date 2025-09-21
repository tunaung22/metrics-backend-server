using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniExcelLibs;

namespace Metrics.Web.Pages.Manage.Submissions.KeyMetrics;

public class IndexModel : PageModel
{
    private readonly Microsoft.Extensions.Configuration.IConfiguration _config;


    private readonly IKeyMetricService _keyMetricService;


    public IndexModel(Microsoft.Extensions.Configuration.IConfiguration config, IKeyMetricService keyMetricService)
    {
        _config = config;
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

    [BindProperty]
    public bool DisplayAll { get; set; } = false;

    [BindProperty]
    public IFormFile UploadedFile { get; set; } = null!;
    public bool UploadSuccess { get; set; }

    // Return URL
    public string? ReturnUrl { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }




    public class KeyKpi
    {
        public string Title { get; set; } = null!;
    }

    // ========== HANDLERS =====================================================
    public async Task<IActionResult> OnGetAsync(
        string? returnUrl,
        [FromQuery] int currentPage,
        [FromQuery] int pageSize)
    {
        // ReturnUrl = returnUrl ??= "Index";
        // TotalDepartments = await _departmentService.FindCountAsync();
        // Departments = await LoadDepartments(CurrentPage, PageSize);
        // CurrentPage = currentPage == 0 ? 1 : currentPage;
        TotalItems = await _keyMetricService.FindCountAsync();
        PageSize = _config.GetValue<int>("Pagination:PageSize");
        if (DisplayAll)
        {
            KeyMetrics = await LoadAllKeyMetrics();
        }
        else
        {
            KeyMetrics = await LoadKeyMetrics(
                currentPage > 0 ? currentPage : CurrentPage,
                pageSize > 0 ? pageSize : PageSize);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostUploadAsync(
        IFormFile file,
        [FromQuery] int currentPage,
        [FromQuery] int pageSize)
    {
        if (file != null && file.Length > 0)
        {
            try
            {
                using (var stream = file.OpenReadStream())
                {
                    var rows = stream.Query<KeyKpi>().ToList();
                    if (rows.Any())
                    {
                        var entitiesToAdd = rows
                            .Where(r => !string.IsNullOrEmpty(r.Title))
                            .Select(r => new KeyMetric
                            {
                                MetricTitle = r.Title.Trim()
                            })
                            .GroupBy(k => k.MetricTitle, StringComparer.OrdinalIgnoreCase)
                            .Select(g => g.First())
                            .ToList();
                        var createdEntities = await _keyMetricService.CreateRangeAsync(entitiesToAdd);
                    }

                    return RedirectToPage();
                }

            }
            catch (System.Exception)
            {
                ModelState.AddModelError(string.Empty, "Problem uploading file.");
                return Page();
            }
        }
        else
        {
            ModelState.AddModelError("", "File not readable.");
        }

        KeyMetrics = await LoadKeyMetrics(
            currentPage > 0 ? currentPage : CurrentPage,
            pageSize > 0 ? pageSize : PageSize);

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
        if (periods.Count() > 0)
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
    private async Task<List<KeyMetricViewModel>> LoadAllKeyMetrics()
    {
        var periods = await _keyMetricService.FindAllAsync();
        if (periods.Count() > 0)
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
