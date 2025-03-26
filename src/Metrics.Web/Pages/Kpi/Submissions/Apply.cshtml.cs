using Metrics.Application.DTOs.DepartmentDtos;
using Metrics.Application.DTOs.KpiSubmissionDtos;
using Metrics.Application.Services.IServices;
using Metrics.Domain.Entities;
using Metrics.Domain.Exceptions;
using Metrics.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace Metrics.Web.Pages.Kpi.Submissions;

public class ApplyModel : PageModel
{
    private readonly IKpiSubmissionService _kpiSubmissionService;
    private readonly IDepartmentService _departmentService;
    private readonly IKpiPeriodService _kpiPeriodService;
    private readonly IEmployeeService _employeeService;

    public ApplyModel(MetricsDbContext context,
                        IDepartmentService departmentService,
                        IKpiPeriodService kpiPeriodService,
                        IKpiSubmissionService kpiSubmissionService,
                        IEmployeeService employeeService)
    {
        _departmentService = departmentService;
        _kpiPeriodService = kpiPeriodService;
        _kpiSubmissionService = kpiSubmissionService;
        _employeeService = employeeService;
    }

    // =========================================================================
    // - fetch all departments
    // - loop department -> render entry form
    // - check existing entry or new entry
    // - on next clicked -> save the entry to session storage
    // - on next clicked -> render new entry from
    // - on submit clicked -> bulk save the entries

    // QueryString: pageNumber = 1, pageSize = 5
    // 1. get employee id
    // 2. get kpi periods
    // 3. get departments
    // 4. get existing submission
    // var totalDepartments = await _departmentService.FindCount_Async();
    // TotalPages = (int)Math.Ceiling(totalDepartments / (double)PageSize);

    // =========================================================================


    [BindProperty]
    public List<SubmissionInputModel> SubmissionInput { get; set; } = [];
    // public SubmissionInputModel SubmissionSingleInput { get; set; } = new SubmissionInputModel();
    // ==========
    public string TargetKpiPeriodName { get; set; } = null!;
    public long TargetKpiPeriodId { get; set; }
    public bool IsSubmissionValid { get; set; } = false; // is current date not early or late
    public long EmployeeId { get; set; }
    public List<DepartmentModel> DepartmentList { get; set; } = [];
    public bool IsSubmissionsExist { get; set; } = false;
    // this model won't need if submission time is based on current time 
    // public DateTimeOffset SubmissionTime { get; set; } = DateTimeOffset.UtcNow; 
    // ==========
    // public bool IsSubmissionAvaiable { get; set; } = false;


    public async Task<IActionResult> OnGetAsync(string periodName)
    {
        TargetKpiPeriodName = periodName;

        // ---------- KPI Period ID ----------------------------------------
        // find kpi period id by period name
        // var periodId = await _kpiPeriodService.FindIdByKpiPeriodName_Async(TargetKpiPeriodName);

        // if (periodId <= 0)
        // {
        //     ModelState.AddModelError("", $"Submission not found for the period {TargetKpiPeriodName}.");
        //     IsSubmissionValid = false;

        //     return Page();
        // }
        // else
        // {
        //     IsSubmissionValid = true;
        //     TargetKpiPeriodId = periodId;
        // }

        // Assign: IsSubmissionValid, TargetKpiPeriodId
        if (!await GetKpiPeriodId(periodName))
            return Page();

        // ---------- Check Today Submission is Valid based on KPI Period ----------
        // var kpiPeriods = await _kpiPeriodService.FindAllByValidDate_Async(DateTimeOffset.UtcNow);
        // if (!kpiPeriods.Any())
        // {
        //     ModelState.AddModelError("", "Submission not avaiable.");
        //     IsSubmissionValid = false;
        //     return Page();
        // }
        // IsSubmissionValid = true;

        // Assign: IsSubmissionValid
        if (!await GetIsSubmissionValid(DateTimeOffset.UtcNow))
            return Page();

        // ---------- Employee ID ----------------------------------------
        EmployeeId = await GetEmployeeId();

        // ----- Load Kpi Period List for Dropdown -----
        // var listItems = await LoadKpiPeriodListItems();
        // if (listItems == null)
        // {
        //     ModelState.AddModelError("", "No KPI Periods Avaiable Currently");
        //     return Page();
        // }
        // KpiPeriodListItems = listItems;
        // SelectedKpiPeriodId = long.TryParse(KpiPeriodListItems[0].Value, out long value) ? value : 0;

        // ---------- DepartmentList ----------------------------------------
        // var departments = await _departmentService.FindAllInsecure_Async();
        // if (!departments.Any())
        // {
        //     ModelState.AddModelError("", "No departments to submit score. Please contact authorities.");
        //     return Page();
        // }
        // DepartmentList = departments.Select(e => new DepartmentModel
        // {
        //     Id = e.Id,
        //     DepartmentCode = e.DepartmentCode,
        //     DepartmentName = e.DepartmentName
        // }).ToList();

        // Assign: DepartmentList
        DepartmentList = await GetDepartmentList();
        if (DepartmentList.Count == 0)
            return Page();



        // ---------- Reinitialize Department List -----------------------------
        // Find submission already exist or not
        // select submissions where employee_id + kpi_period_id + department_id
        // var departmentIDs = DepartmentList.Select(department => department.Id).ToList<long>();
        // var existingSubmissions = await _kpiSubmissionService.Find_Async(EmployeeId, TargetKpiPeriodId, departmentIDs);
        // if (existingSubmissions.Any())
        // {
        //     if (DepartmentList.Count == existingSubmissions.Count())
        //     {
        //         // ModelState.AddModelError("", "Already Submitted.");
        //         IsSubmissionsExist = true;
        //         return Page();
        //     }

        //     // ============================================================
        //     // var a = from d in DepartmentList
        //     //         join s in existingSubmissions
        //     //         on d.Id equals s.DepartmentId into submissionsGroup
        //     //         from s in submissionsGroup.DefaultIfEmpty() // Left join
        //     //         where s == null // Filter for departments with no submissions
        //     //         select d;
        //     // DepartmentList = a.ToList();
        //     // ============================================================

        //     // Filter DepartmentList only for new Submissions
        //     DepartmentList = DepartmentList
        //         .Where(d => !existingSubmissions.Any(s => s.DepartmentId == d.Id))
        //         .ToList();
        // }

        // 1. all submissions already exist/fullfilled, no need to submit anymore.
        // 2. part of previous submissions found, but can submit for other departments that were added later time after first submission.
        // 3. no previous submissions found, need to submit.
        var existingSubmissions = await GetExistingSubmissions(DepartmentList);
        if (existingSubmissions != null)
        {
            // all submission already exist/fullfilled
            if (DepartmentList.Count == existingSubmissions.Count)
            {
                ModelState.AddModelError("", "Already Submitted.");
                IsSubmissionsExist = true;

                return Page();
            }

            DepartmentList = await UpdateDepartmentList(DepartmentList, existingSubmissions);
        }
        // no previous submissions found (continue)

        // ---------- Bind Input Model List ------------------------------------
        SubmissionInput = new List<SubmissionInputModel>();
        SubmissionInput = DepartmentList.Select(department => new SubmissionInputModel
        {
            // EmployeeId = EmployeeId,
            // KpiPeriodId = TargetKpiPeriodId,
            // DepartmentId = department.Id,
            KpiScore = 5
        }).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string periodName)
    {
        TargetKpiPeriodName = periodName;

        // Assign: IsSubmissionValid, TargetKpiPeriodId
        if (!await GetKpiPeriodId(periodName))
            return Page();

        // Assign: IsSubmissionValid
        if (!await GetIsSubmissionValid(DateTimeOffset.UtcNow))
            return Page();

        EmployeeId = await GetEmployeeId();

        // Assign: DepartmentList
        DepartmentList = await GetDepartmentList();
        if (DepartmentList.Count == 0)
            return Page();

        // 1. all submissions already exist/fullfilled, no need to submit anymore.
        // 2. part of previous submissions found, but can submit for other departments that were added later time after first submission.
        // 3. no previous submissions found, need to submit.
        var existingSubmissions = await GetExistingSubmissions(DepartmentList);
        if (existingSubmissions != null)
        {
            // all submission already exist/fullfilled
            if (DepartmentList.Count == existingSubmissions.Count)
            {
                ModelState.AddModelError("", "Already Submitted.");
                IsSubmissionsExist = true;

                return Page();
            }

            DepartmentList = await UpdateDepartmentList(DepartmentList, existingSubmissions);
        }
        // no previous submissions found (continue)



        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("", "Form invalid");

            return Page();
        }


        // if (DepartmentList.Count != SubmissionInput.Count)
        // {
        //     ModelState.AddModelError("", "Department list and submission list mismatch. There will be chances of changes in departments data while you're filling the form. Please try again.");
        //     return Page();
        // }

        var createDto = new List<KpiSubmissionCreateDto>();

        for (int i = 0; i < SubmissionInput.Count; i++)
        {
            var dto = new KpiSubmissionCreateDto
            {
                SubmissionTime = DateTimeOffset.UtcNow,
                EmployeeId = EmployeeId,
                KpiPeriodId = TargetKpiPeriodId,
                DepartmentId = DepartmentList[i].Id,
                KpiScore = SubmissionInput[i].KpiScore.Equals(0) ? 1 : SubmissionInput[i].KpiScore,
                Comments = SubmissionInput[i].Comments ?? ""
            };

            createDto.Add(dto);
        }

        try
        {
            var appliedRecords = await _kpiSubmissionService.CreateRange_Async(createDto);

            HttpContext.Session.SetString("SubmissionSuccessful", "true");
            HttpContext.Session.SetString("TargetKpiPeriodName", TargetKpiPeriodName);

            return RedirectToPage("Success");

        }
        catch (DuplicateContentException)
        {
            ModelState.AddModelError("", "Already submitted for current period.");
            return Page();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return Page();
        }
    }

    // ========== Methods ==================================================
    private async Task<bool> GetKpiPeriodId(string kpiPeriodName)
    {
        var periodId = await _kpiPeriodService.FindIdByKpiPeriodName_Async(kpiPeriodName);

        if (periodId > 0)
        {
            IsSubmissionValid = true;
            TargetKpiPeriodId = periodId;

            return true;
        }

        ModelState.AddModelError("", $"Submission not found for the period {kpiPeriodName}.");
        IsSubmissionValid = false;

        return false;
    }

    private async Task<bool> GetIsSubmissionValid(DateTimeOffset submissionTime)
    {
        var kpiPeriods = await _kpiPeriodService.FindAllByValidDate_Async(submissionTime.UtcDateTime);

        if (!kpiPeriods.Any())
        {
            ModelState.AddModelError("", "Submission not avaiable.");
            IsSubmissionValid = false;

            return false;
        }

        IsSubmissionValid = true;
        return true;
    }

    private async Task<long> GetEmployeeId()
    {
        // Less likely to cause user not found, so throw just in case
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new Exception("User not found. Please login again.");

        return await _employeeService.FindEmployeeId_Async(userId);
    }

    private async Task<List<DepartmentModel>> GetDepartmentList()
    {
        var departments = await _departmentService.FindAllInsecure_Async();
        if (departments.Any())
        {
            return departments.Select(e => new DepartmentModel
            {
                Id = e.Id,
                DepartmentCode = e.DepartmentCode,
                DepartmentName = e.DepartmentName
            }).ToList();
        }

        ModelState.AddModelError("", "No departments to submit score. Please contact authorities.");
        return [];
    }

    private async Task<List<KpiSubmissionDto>> GetExistingSubmissions(List<DepartmentModel> departments)
    {
        var departmentIDs = departments.Select(department => department.Id).ToList<long>();
        var existingSubmissions = await _kpiSubmissionService.Find_Async(EmployeeId, TargetKpiPeriodId, departmentIDs);

        return existingSubmissions.ToList();
    }


    private Task<List<DepartmentModel>> UpdateDepartmentList(
        List<DepartmentModel> departmentList,
        List<KpiSubmissionDto> existingSubmissions)
    {
        // ============================================================
        // var a = from d in DepartmentList
        //         join s in existingSubmissions
        //         on d.Id equals s.DepartmentId into submissionsGroup
        //         from s in submissionsGroup.DefaultIfEmpty() // Left join
        //         where s == null // Filter for departments with no submissions
        //         select d;
        // DepartmentList = a.ToList();
        // ============================================================

        // part of previous submissions found
        // Filter DepartmentList only for new Submissions
        var result = departmentList
            .Where(d => !existingSubmissions.Any(s => s.DepartmentId == d.Id))
            .ToList();

        return Task.FromResult(result);
    }

    // TODO: Unused method
    private async Task<List<SelectListItem>> LoadKpiPeriodListItems()
    {
        var kpiPeriods = await _kpiPeriodService.FindAllByValidDate_Async(DateTimeOffset.UtcNow);

        if (!kpiPeriods.Any())
            return [];

        return kpiPeriods.Select(e => new SelectListItem
        {
            Value = e.Id.ToString(),
            Text = $"{e.PeriodName} ({e.SubmissionStartDate:dd/MMM/yyyy} - {e.SubmissionEndDate:dd/MMM/yyyy})"
        }).ToList();
    }





    // ========== Models ==========
    public class SubmissionInputModel
    {
        // public long Id { get; set; }
        public DateTimeOffset SubmissionTime { get; set; }
        [Required(ErrorMessage = "KPI Score is required.")]
        [Range(1, 10, ErrorMessage = "KPI Score is required and choose between 1 to 10.")]
        public decimal KpiScore { get; set; }
        public string? Comments { get; set; }
        // Foreign Keys
        public long KpiPeriodId { get; set; }
        [Required(ErrorMessage = "Department ID is required.")]
        public long DepartmentId { get; set; }
        public long EmployeeId { get; set; }
        // Reference Navigational Properties
        // public KpiPeriod KpiPeriod { get; set; } = null!;
        // public Department TargetDepartment { get; set; } = null!;
        // public Employee Candidate { get; set; } = null!;
    }
    public class DepartmentModel
    {
        public long Id { get; set; }
        public Guid DepartmentCode { get; set; }
        public string DepartmentName { get; set; } = null!;
    }
}
