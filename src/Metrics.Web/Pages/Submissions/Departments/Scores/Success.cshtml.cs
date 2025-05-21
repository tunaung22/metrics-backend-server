using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metrics.Web.Pages.Submissions.Departments.Scores;

public class SuccessModel : PageModel
{
    public string TargetKpiPeriodName { get; set; } = string.Empty;
    // public bool SubmissionSuccessful {get;set; } = false;

    public IActionResult OnGet()
    {
        // TargetKpiPeriodName = HttpContext.Session.GetString("TargetKpiPeriodName") ?? string.Empty;
        // var submissionSuccessful = HttpContext.Session.GetString("SubmissionSuccessful");


        // if (string.IsNullOrEmpty(submissionSuccessful))
        // {
        //     return RedirectToPage("/Error/404");
        // }

        // HttpContext.Session.Remove("SubmissionSuccessful");
        // HttpContext.Session.Remove("TargetKpiPeriodName");
        // var success = TempData["SubmissionSuccessful"] as bool? ?? false;
        // if (!success)
        // {
        //     return RedirectToPage("/Error/404");
        // }

        TargetKpiPeriodName = TempData["TargetKpiPeriodName"] as string ?? string.Empty;
        if (string.IsNullOrEmpty(TargetKpiPeriodName))
        {
            return RedirectToPage("Index");
        }

        return Page();
    }
}
