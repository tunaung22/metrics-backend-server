using Metrics.Application.Authorization;
using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Common.Mappers;
using Metrics.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics.Eventing.Reader;
using System.Security.Claims;

namespace Metrics.Web.Pages.Submissions.DepartmentMetricScores;

[Authorize(Policy = ApplicationPolicies.CanSubmit_KeyKpiScore_Policy)]
public class IndexModel(
    IUserService userService,
    // IDepartmentService departmentService,
    IKeyKpiSubmissionService keyKpiSubmissionService,
    IKeyKpiSubmissionConstraintService keyKpiSubmissionConstraintService,
    IKpiSubmissionPeriodService kpiSubmissionPeriodService,
    IDepartmentKeyMetricService dkmService) : PageModel
{
    private readonly IUserService _userService = userService;
    // private readonly IDepartmentService _departmentService = departmentService;
    private readonly IKeyKpiSubmissionService _keyKpiSubmissionService = keyKpiSubmissionService;
    private readonly IKeyKpiSubmissionConstraintService _keyKpiSubmissionConstraintService = keyKpiSubmissionConstraintService;
    private readonly IKpiSubmissionPeriodService _kpiSubmissionPeriodService = kpiSubmissionPeriodService;
    private readonly IDepartmentKeyMetricService _dkmService = dkmService;

    // ========== MODELS =======================================================
    public class SubmissionPeriodModel // Overview info of submission for the Period
    {
        // public long Id { get; set; }
        public string PeriodName { get; set; } = string.Empty;
        public DateTimeOffset SubmissionStartDate { get; set; }
        public DateTimeOffset SubmissionEndDate { get; set; }
        public bool IsSubmitted { get; set; } = false; // complete/in-progress
        public bool IsValid { get; set; } = false; // enable,disable
    }
    public List<SubmissionPeriodModel> SubmissionPeriods { get; set; } = []; // model for KPI period table

    public bool IsSubmissionDateValid { get; set; } = false;
    public List<KpiPeriodViewModel> KpiPeriods { get; set; } = []; // model for KPI period table
    public UserViewModel Submitter { get; set; } = null!;


    // ========== HANDLERS =====================================================
    /// <summary>
    /// OnGet
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<IActionResult> OnGetAsync()
    {
        // ----------KPI PERIOD-------------------------------------------------
        var periodList = await LoadKpiPeriods();
        if (periodList == null || periodList.Count == 0)
            return Page();

        KpiPeriods = periodList.ToList();


        // var kpiPeriodList = (await _kpiSubmissionPeriodService.FindAllAsync()).ToList();
        // if (kpiPeriodList.Count <= 0)
        // {
        //     IsSubmissionAvaiable = false;
        //     return Page();
        // }
        // IsSubmissionAvaiable = true;

        // ----------CURRENT USER aka SUBMITTER----------------------------------------------
        var submitter = await GetCurrentUser();
        if (submitter == null)
            return Page();

        Submitter = submitter;
        // CurrentUserGroupName = Submitter.UserGroup.GroupName ?? string.Empty;
        // SubmitterDepartment = submitter.Department;

        // Check submissions is completed
        // parent   item count :: can compare with department count (based on period)
        // child    item count :: can compare with DKM's department count (based on period)
        // foreach period:
        //          get submissions where submissions.period == period
        //              submission's items .departments
        // - where::submissionConstraints.DKM.period == period 
        // submission's department count == 
        // - submissions.department.count

        var periodIds = KpiPeriods.Select(x => x.Id).ToList();

        var submissionsCount = await _keyKpiSubmissionService
            .FindSubmissionsCountDictByPeriodBySubmitterAsync(periodIds, Submitter.Id);
        if (!submissionsCount.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, "Failed to get period list, Please try again.");
            return Page();
        }
        var tmp = await _keyKpiSubmissionConstraintService.FindCountsByPeriodBySubmitterDepartmentAsync(
            periodIds, Submitter.Department.Id);
        // var dkmsCount = await _dkmService.FindCountsByPeriodBySubmitterAsync(periodIds, Submitter.Id);
        var dkmsCount = tmp;
        if (!dkmsCount.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, "Failed to get period list, Please try again.");
            return Page();
        }

        SubmissionPeriods = KpiPeriods.Select(period =>
        {
            // ---------- CHECK PERIOD DATE IS VALID OR NOT ----------------------
            IsSubmissionDateValid = CheckSubmissionDateValidity(
                startDate: period.SubmissionStartDate,
                endDate: period.SubmissionEndDate);
            // ---------- CHECK SUBMISSION EXIST----------------------
            // submission count >= dkm count    => valid
            // submission count < dkm count     => partial
            // no submission count              => new 
            var submissionCountByPeriod = submissionsCount.Data?.GetValueOrDefault(period.Id, 0);
            var dkmCountByPeriod = dkmsCount.Data?.GetValueOrDefault(period.Id, 0);
            // new      -> enable           text: Submit
            // update   -> enable           text: Submit
            // Finished -> enable           text: View
            // Due      -> disable          text: - 


            // if (submissionCountByPeriod >= dkmCountByPeriod)
            // {
            //     // DONE
            //     return new SubmissionPeriodModel
            //     {
            //         PeriodName = period.PeriodName,
            //         SubmissionStartDate = period.SubmissionStartDate,
            //         SubmissionEndDate = period.SubmissionEndDate,
            //         IsSubmitted = true,
            //         IsValid = IsSubmissionDateValid,
            //     };
            // }
            // else
            // {
            //     // NEW or PARTIAL
            //     return new SubmissionPeriodModel
            //     {
            //         PeriodName = period.PeriodName,
            //         SubmissionStartDate = period.SubmissionStartDate,
            //         SubmissionEndDate = period.SubmissionEndDate,
            //         IsSubmitted = false,
            //         IsValid = IsSubmissionDateValid,
            //     };
            // }


            if (submissionCountByPeriod < dkmCountByPeriod || submissionCountByPeriod == 0)
            {
                // NEW or PARTIAL
                return new SubmissionPeriodModel
                {
                    PeriodName = period.PeriodName,
                    SubmissionStartDate = period.SubmissionStartDate,
                    SubmissionEndDate = period.SubmissionEndDate,
                    IsSubmitted = false,
                    IsValid = IsSubmissionDateValid,
                };
            }
            // DONE
            return new SubmissionPeriodModel
            {
                PeriodName = period.PeriodName,
                SubmissionStartDate = period.SubmissionStartDate,
                SubmissionEndDate = period.SubmissionEndDate,
                IsSubmitted = true,
                IsValid = IsSubmissionDateValid,
            };
        }).ToList();

        // ---------- CHECK SUBMISSION AVAIBLE----------------------

        return Page();
    }

    // KpiPeriodList = KpiPeriods.Select(period =>
    //     {
    //         // ---------- CHECK PERIOD DATE IS VALID OR NOT ----------------------
    //         IsSubmissionDateValid = CheckSubmissionDateValidity(
    //             startDate: period.SubmissionStartDate,
    //             endDate: period.SubmissionEndDate);
    //         // ---------- CHECK SUBMISSION EXIST----------------------
    //         // submission count == dkm count    => valid
    //         // submission count < dkm count     => partial
    //         // no submission count              => new 
    //         var submissionCountByPeriod = submissionsCount.Data?.GetValueOrDefault(period.Id, 0);
    //         var dkmCountByPeriod = dkmsCount.Data?.GetValueOrDefault(period.Id, 0);
    //         // button status
    //         // new      -> enable           text: Submit
    //         // update   -> enable           text: Submit
    //         // Finished -> enable           text: View
    //         // Due      -> disable          text: - 
    //         // IsEnabled -> Submit, View        new, update, completed
    //         // !IsEnabled -> -                  due, old
    //         if (IsSubmissionDateValid)
    //         {
    //             if (submissionCountByPeriod == 0)
    //                 // enable
    //                 return new PeriodListViewModel
    //                 {
    //                     KpiPeriod = period,
    //                     Status = "New",
    //                     Enabled = true,
    //                 };
    //             else
    //             {
    //                 if (submissionCountByPeriod >= dkmCountByPeriod)
    //                 {
    //                     return new PeriodListViewModel
    //                     {
    //                         KpiPeriod = period,
    //                         Status = "Completed",
    //                         Enabled = true,
    //                     };
    //                 }
    //                 else // (submissionCountByPeriod < dkmCountByPeriod)
    //                 {
    //                     return new PeriodListViewModel
    //                     {
    //                         KpiPeriod = period,
    //                         Status = "Update",
    //                         Enabled = true,
    //                     };
    //                 }
    //             }
    //         }
    //         else
    //         {
    //             return new PeriodListViewModel
    //             {
    //                 KpiPeriod = period,
    //                 Status = "-",
    //                 Enabled = false
    //             };
    //         }


    // Tmp.Add(new TempModel
    // {
    //     Period = period.PeriodName,
    //     Status = Message
    // });
    // if (!IsSubmissionDateValid)
    //     return Page();
    //  submission
    // var departmentList = await _departmentService.FindAllAsync();

    // DKM by Period -> get Submissions
    // var dkms = _dkmService.FindByPeriodAsync(period.Id);
    // dkms
    // get Constraints by Department of CurrentUser
    // var submissionConstraints = await _keyKpiSubmissionConstraintService
    //     .FindByPeriodBySubmitterDepartmentAsync(
    //         periodId: period.Id,
    //         submitterDepartmenCode: currentUser.Department.DepartmentCode
    //     );
    // if (submissionConstraints.IsSuccess)
    // {
    //     if (submissionConstraints.Data?.Count > 0)
    //     {
    //         // list of period 
    //     }
    // }
    // var submissionByPeriodBySubmitter = await _keyKpiSubmissionService
    //     .FindByPeriodBySubmitterAsync(period.Id, userId);
    // if (submissionByPeriodBySubmitter.IsSuccess)
    // {
    //     if (submissionByPeriodBySubmitter.Data != null)
    //     {
    //         // if(submission count == dkms count) => finished
    //         // else => in-progress
    //         // invalid submission (date is dued)
    //         SubmissionPeriods.Add(new KpiSubmissionPeriodModel
    //         {
    //             PeriodName = period.PeriodName,
    //             SubmissionStartDate = period.SubmissionStartDate,
    //             SubmissionEndDate = period.SubmissionEndDate,
    //             IsSubmitted = false,
    //             IsValid = isPeriodValid
    //         });
    //     }
    // }



    // =============== METHODS =================================================
    private async Task<List<KpiPeriodViewModel>> LoadKpiPeriods()
    {
        var periods = await _kpiSubmissionPeriodService.FindAllAsync();

        return periods.Select(p => new KpiPeriodViewModel
        {
            Id = p.Id,
            PeriodName = p.PeriodName,
            SubmissionStartDate = p.SubmissionStartDate,
            SubmissionEndDate = p.SubmissionEndDate
        }).ToList();
    }

    private async Task<UserViewModel?> GetCurrentUser()
    {
        // var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier)
        //     ?? throw new InvalidOperationException("User is not authenticated.");
        // var currentUser = await _userService.FindByIdAsync(userId);
        UserViewModel? data = null;

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Less likely to cause user not found, so throw just in case
            // ?? throw new Exception("User not found. Please login again.");
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userService.FindByIdAsync_2(userId);
                if (user.IsSuccess && user.Data != null)
                    data = user.Data.MapToViewModel();
            }
            else
                ModelState.AddModelError(string.Empty, "User not found. Please login again.");

            return data;
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Invalid current user.");
            return data;
        }
    }

    /// <summary>
    /// Check Submission Date Validity
    /// date is early or due
    /// </summary>
    /// early: dt < start, late: dt > end
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <returns></returns>
    private bool CheckSubmissionDateValidity(
        DateTimeOffset startDate,
        DateTimeOffset endDate)
    {
        // var UTC_NOW = DateTimeOffset.Now.UtcDateTime;
        // var isPeriodValid = UTC_NOW > period.SubmissionStartDate
        //     && UTC_NOW < period.SubmissionEndDate;
        // EARLY => dt < start date
        if (DateTime.Now < startDate)
        {
            // ModelState.AddModelError(string.Empty, "Submission not open yet.");
            return false;
        }
        // DUE   => dt > end date
        else if (DateTime.Now > endDate)
        {
            // ModelState.AddModelError(string.Empty, "Submission not open yet.");
            return false;
        }

        return true;
    }
}


//     // constraints -> departments   -> existing Submissions
//     // constraints -> dkms          -> submision status
//     if (submissionConstraints.Any())
//     {
//         var dkms = submissionConstraints
//             .OrderBy(c => c.DepartmentId)
//             .Select(c => c.DepartmentKeyMetric);

//         var submitterDepartments = submissionConstraints
//             .Select(c => c.DepartmentKeyMetric.KeyIssueDepartment)
//             .DistinctBy(department => department.Id)
//             .ToList();

//         var existingSubmissions = await _keyKpiSubmissionService
//             .FindByDepartmentKeyMetricsAsync(dkms.Select(dkm => dkm.Id).ToList());

//         // .FindBySubmitterByPeriodByDepartmentListAsync(
//         //     currentUser,
//         //     period.Id,
//         //     submitterDepartments.Select(d => d.Id).ToList()
//         // );
//         if (existingSubmissions.IsSuccess)
//         {
//             var count = existingSubmissions.Data?.Count;
//             // finished or in-progress
//             if (count == dkms.Count())
//             {
//                 // finished
//                 SubmissionPeriods.Add(new KpiSubmissionPeriodModel
//                 {
//                     PeriodName = period.PeriodName,
//                     SubmissionStartDate = period.SubmissionStartDate,
//                     SubmissionEndDate = period.SubmissionEndDate,
//                     IsSubmitted = true,
//                     IsValid = isPeriodValid
//                 });
//             }
//             else
//             {
//                 // in-progress
//                 SubmissionPeriods.Add(new KpiSubmissionPeriodModel
//                 {
//                     PeriodName = period.PeriodName,
//                     SubmissionStartDate = period.SubmissionStartDate,
//                     SubmissionEndDate = period.SubmissionEndDate,
//                     IsSubmitted = false,
//                     IsValid = isPeriodValid
//                 });
//             }
//         }
//         else
//         {
//             // new submission
//             SubmissionPeriods.Add(new KpiSubmissionPeriodModel
//             {
//                 PeriodName = period.PeriodName,
//                 SubmissionStartDate = period.SubmissionStartDate,
//                 SubmissionEndDate = period.SubmissionEndDate,
//                 IsSubmitted = false,
//                 IsValid = isPeriodValid
//             });
//         }

//     }
//     else
//     {
//         // invalid submission (date is dued)
//         SubmissionPeriods.Add(new KpiSubmissionPeriodModel
//         {
//             PeriodName = period.PeriodName,
//             SubmissionStartDate = period.SubmissionStartDate,
//             SubmissionEndDate = period.SubmissionEndDate,
//             IsSubmitted = false,
//             IsValid = isPeriodValid
//         });
//     }

// }