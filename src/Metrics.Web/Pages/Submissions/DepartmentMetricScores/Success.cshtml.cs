using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metrics.Web.Pages.Submissions.DepartmentMetricScores;

public class SuccessModel : PageModel
{
    public string TargetKpiPeriodName { get; set; } = string.Empty;

    public IActionResult OnGet()
    {
        TargetKpiPeriodName = TempData["TargetKpiPeriodName"] as string ?? string.Empty;
        if (string.IsNullOrEmpty(TargetKpiPeriodName))
        {
            return RedirectToPage("Index");
        }

        return Page();
    }
}
