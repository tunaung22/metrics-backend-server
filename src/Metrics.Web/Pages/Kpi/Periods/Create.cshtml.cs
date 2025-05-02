using Metrics.Application.Domains;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Metrics.Web.Pages.Kpi.Periods;

public class CreateModel : PageModel
{
    private readonly IKpiPeriodService _kpiPeriodService;

    public CreateModel(IKpiPeriodService kpiPeriodService)
    {
        _kpiPeriodService = kpiPeriodService;
    }

    // ========== MODELS =======================================================
    public class FormInputModel
    {
        [Required(ErrorMessage = "Period Name is required.")]
        [StringLength(7, MinimumLength = 7, ErrorMessage = "Invalid name pattern. Period name must be 4 numbers + - + 2 numbers.")]
        [RegularExpression(@"^\d{4}-\d{2}$", ErrorMessage = "The date must be in the format YYYY-MM.")]
        public string PeriodName { get; set; } = null!;

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MMM/yyyy HH:mm}")]
        [DataType(DataType.DateTime)]
        public DateTimeOffset SubmissionStartDate { get; set; } = DateTimeOffset.UtcNow.UtcDateTime;

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MMM/yyyy}")]
        [DataType(DataType.DateTime)]
        public DateTimeOffset SubmissionEndDate { get; set; } = DateTimeOffset.UtcNow.UtcDateTime;
    }

    // ========== Binding Models ===============================================
    [BindProperty]
    public FormInputModel FormInput { get; set; } = new FormInputModel();
    public string ReturnUrl { get; set; } = string.Empty;

    // ========== HANDLERS =====================================================
    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            // var createDto = new KpiPeriodCreateDto
            // {
            //     PeriodName = FormInput.PeriodName,
            //     SubmissionStartDate = FormInput.SubmissionStartDate,
            //     SubmissionEndDate = FormInput.SubmissionEndDate
            // };
            // await _kpiPeriodService.Create_Async(createDto);
            var entity = new KpiPeriod
            {
                PeriodName = FormInput.PeriodName,
                SubmissionStartDate = FormInput.SubmissionStartDate.UtcDateTime,
                SubmissionEndDate = FormInput.SubmissionEndDate.UtcDateTime
            };
            await _kpiPeriodService.CreateAsync(entity);
            return RedirectToPage("Index");

        }
        catch (DuplicateContentException)
        {
            ModelState.AddModelError("DuplicateContent", "Duplicate KPI Period");
        }
        catch (Exception)
        {
            throw;
        }

        return Page();

    }

    public IActionResult OnPostCancel()
    {
        if (!string.IsNullOrEmpty(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }
        return RedirectToPage("./Index");
    }


    public async Task<IActionResult> OnGetCheckKpiNameAsync(string value)
    {
        var kpiPeriodNameExists = await _kpiPeriodService.KpiPeriodNameExistsAsync(value);

        // return new JsonResult(new { 
        //     message = kpiPeriodNameExists ? "" : ""
        // });
        if (kpiPeriodNameExists)
        {
            // Conflict
            // return new JsonResult(new
            // {
            //     message = "Period name already exist."
            // });
            return StatusCode(StatusCodes.Status409Conflict, new { message = "Period name already exist." });
            // { StatusCode = (int?)HttpStatusCode.Conflict };
            // return Conflict(new { message = "The period name is already in use." });
        }

        // return new JsonResult(new
        // {
        //     message = "Avaiable"
        // })
        // { StatusCode = (int?)HttpStatusCode.OK };
        // return new JsonResult(new { message = "The period name is available." });
        return StatusCode(StatusCodes.Status200OK, new { message = "The period name is available." });
    }


}
