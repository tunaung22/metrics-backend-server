using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metrics.Web.Pages.Reports.Submissions.Departments.Kpi;

[Authorize(Policy = "CanAccessAdminFeaturePolicy")]
public class ViewModel : PageModel
{
    private readonly IKpiSubmissionPeriodService _kpiPeriodService;
    private readonly IDepartmentService _departmentService;
    private readonly IKpiSubmissionService _kpiSubmissionService;

    public ViewModel(
        IKpiSubmissionPeriodService kpiPeriodService,
        IDepartmentService departmentService,
        IKpiSubmissionService kpiSubmissionService)
    {
        _kpiPeriodService = kpiPeriodService;
        _departmentService = departmentService;
        _kpiSubmissionService = kpiSubmissionService;
    }

    // =============== MODELS ==================================================
    public class KpiReportModel
    {
        public required string KpiPeriodName { get; set; }
        public required string DepartmentName { get; set; }
        public required int TotalReceivedSubmissions { get; set; } = 0;
        public required decimal TotalScoreReceived { get; set; } = 0M;
        public required decimal FinalKpiScore { get; set; } = 0M;
    }

    public List<DepartmentViewModel> DepartmentList { get; set; } = new List<DepartmentViewModel>();
    public List<KpiPeriodViewModel> KpiPeriodlist { get; set; } = new List<KpiPeriodViewModel>();
    public List<KpiReportModel> KpiReportList { get; set; } = new List<KpiReportModel>();
    public KpiPeriodViewModel SelectedPeriod { get; set; } = new KpiPeriodViewModel()
    {
        PeriodName = DateTime.UtcNow.Date.Year.ToString() + DateTime.UtcNow.Date.Month.ToString("MM")
    };
    // =============== HANDLERS ================================================

    public async Task<IActionResult> OnGetAsync(string periodName)
    {
        // CHECK periodName exist in submissions
        // GET id, SET id
        var kpiPeriod = await _kpiPeriodService.FindByKpiPeriodNameAsync(periodName);
        if (kpiPeriod != null)
            SelectedPeriod = new KpiPeriodViewModel()
            {
                Id = kpiPeriod.Id,
                PeriodName = kpiPeriod.PeriodName,
                SubmissionStartDate = kpiPeriod.SubmissionStartDate,
                SubmissionEndDate = kpiPeriod.SubmissionEndDate
            };
        else
            ModelState.AddModelError("", $"Submission not found for the period {periodName}.");


        var departments = await _departmentService.FindAllAsync();
        if (departments.Any())
        {
            DepartmentList = departments.Select(d => new DepartmentViewModel()
            {
                Id = d.Id,
                DepartmentName = d.DepartmentName,
                DepartmentCode = d.DepartmentCode
            }).ToList();

            // ----- List Submission for **SELECTED KPI Period ----------
            foreach (var department in DepartmentList)
            {
                var submissionListByPeriodAndDepartment = await _kpiSubmissionService
                    .FindByKpiPeriodAndDepartmentAsync(SelectedPeriod.Id, department.Id);

                if (submissionListByPeriodAndDepartment.Any())
                {
                    var totalSubmission = submissionListByPeriodAndDepartment.Count;
                    var totalScore = submissionListByPeriodAndDepartment.Sum(e => e.ScoreValue);

                    var singleReportForPeriodAndDepartment = new KpiReportModel()
                    {
                        KpiPeriodName = SelectedPeriod.PeriodName,
                        DepartmentName = department.DepartmentName,
                        TotalReceivedSubmissions = totalSubmission,
                        TotalScoreReceived = totalScore,
                        FinalKpiScore = totalScore / totalSubmission
                    };

                    KpiReportList.Add(singleReportForPeriodAndDepartment);
                }
            }
        }



        // ----- List Submission for **ALL KPI Periods ----------
        // all kpi periods
        // var kpiPeriods = await _kpiPeriodService.FindAllAsync();
        // KpiPeriodlist = kpiPeriods.Select(p => new KpiPeriodViewModel()
        // {
        //     Id = p.Id,
        //     PeriodName = p.PeriodName,
        //     SubmissionStartDate = p.SubmissionStartDate,
        //     SubmissionEndDate = p.SubmissionEndDate
        // }).ToList();

        // foreach (var period in KpiPeriodlist)
        // {
        //     foreach (var department in DepartmentList)
        //     {
        //         var submissionListByPeriodAndDepartment = await _kpiSubmissionService
        //             .FindByKpiPeriodAndDepartmentAsync(period.Id, department.Id);

        //         if (submissionListByPeriodAndDepartment.Any())
        //         {
        //             var totalSubmission = submissionListByPeriodAndDepartment.Count;
        //             var totalScore = submissionListByPeriodAndDepartment.Sum(e => e.KpiScore);

        //             var singleReportForPeriodAndDepartment = new KpiReportModel()
        //             {
        //                 KpiPeriodName = period.PeriodName,
        //                 DepartmentName = department.DepartmentName,
        //                 TotalReceivedSubmissions = totalSubmission,
        //                 TotalScoreReceived = totalScore,
        //                 FinalKpiScore = totalSubmission / totalScore
        //             };

        //             KpiReportList.Add(singleReportForPeriodAndDepartment);
        //         }
        //     }
        // }

        return Page();
    }

    // ========== Methods ==================================================

}
