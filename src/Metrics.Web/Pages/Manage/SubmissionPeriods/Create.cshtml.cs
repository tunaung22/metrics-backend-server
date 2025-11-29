using Metrics.Application.DTOs.KpiPeriod;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Metrics.Web.Pages.Manage.SubmissionPeriods;

public class CreateModel : PageModel
{
    private readonly ILogger<CreateModel> _logger;
    private readonly IKpiSubmissionPeriodService _kpiPeriodService;

    public CreateModel(ILogger<CreateModel> logger, IKpiSubmissionPeriodService kpiPeriodService)
    {
        _logger = logger;
        _kpiPeriodService = kpiPeriodService;
    }

    // ========== MODELS =======================================================
    public class FormInputModel
    {
        [Required(ErrorMessage = "Period Name is required.")]
        // [StringLength(7, MinimumLength = 7, ErrorMessage = "Invalid name pattern. Period name must be 4 numbers + - + 2 numbers.")]
        // [RegularExpression(@"^\d{4}-\d{2}$", ErrorMessage = "The date must be in the format YYYY-MM.")]
        public string PeriodName { get; set; } = null!;

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MMM/yyyy HH:mm}")]
        [DataType(DataType.DateTime)]
        public DateTimeOffset SubmissionStartDate { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MMM/yyyy}")]
        [DataType(DataType.DateTime)]
        public DateTimeOffset SubmissionEndDate { get; set; }
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
            // var startDate = new DateTimeOffset(FormInput.SubmissionStartDate.Date.AddHours(6));
            // var endDate = new DateTimeOffset(FormInput.SubmissionEndDate.Date.AddDays(1).AddSeconds(-1));
            var startDate = new DateTimeOffset(FormInput.SubmissionStartDate.Date);
            var endDate = new DateTimeOffset(FormInput.SubmissionEndDate.Date.AddDays(1).AddSeconds(-1)); // end of the day

            if (startDate > endDate)
            {
                ModelState.AddModelError(string.Empty, "Invalid Start Date and End Date");
                return Page();
            }

            var createDto = new KpiPeriodCreateDto
            {
                PeriodName = FormInput.PeriodName,
                SubmissionStartDate = startDate,
                SubmissionEndDate = endDate
            };
            var result = await _kpiPeriodService.CreateAsync(createDto);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, "Failed to create kpi period.");
                return Page();
            }

            return RedirectToPage("Index");
        }
        catch (DuplicateContentException)
        {
            ModelState.AddModelError("DuplicateContent", "Duplicate KPI Period");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            ModelState.AddModelError("string.Empty", "Error adding KPI Period.");
            return Page();
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
