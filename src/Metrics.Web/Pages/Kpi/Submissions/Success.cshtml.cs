using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metrics.Web.Pages.Kpi.Submissions
{
    public class SuccessModel : PageModel
    {
        public string TargetKpiPeriodName { get; set; } = string.Empty;
        // public bool SubmissionSuccessful {get;set; } = false;

        public IActionResult OnGet()
        {
            TargetKpiPeriodName = HttpContext.Session.GetString("TargetKpiPeriodName") ?? string.Empty;
            var submissionSuccessful = HttpContext.Session.GetString("SubmissionSuccessful");

            if (string.IsNullOrEmpty(submissionSuccessful))
            {
                return RedirectToPage("/Errors/404");
            }

            HttpContext.Session.Remove("SubmissionSuccessful");
            HttpContext.Session.Remove("TargetKpiPeriodName");

            return Page();
        }
    }
}
