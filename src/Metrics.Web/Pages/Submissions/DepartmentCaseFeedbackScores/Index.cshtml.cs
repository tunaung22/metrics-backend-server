using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metrics.Web.Pages.Submissions.DepartmentCaseFeedbackScores;

[Authorize(Policy = "CanSubmitCaseFeedbackPolicy")]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
