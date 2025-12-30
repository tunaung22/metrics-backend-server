using Metrics.Application.DTOs.Reports.KeyKpiSubmissionReports;
using Metrics.Web.Models;
using Metrics.Web.Models.ReportViewModels.KeyKpiSummaryReports;

namespace Metrics.Web.Common.Mappers;

public static class KeyKpiSubmissionReportMapper
{
    // ---SummaryReport_Multiple to KeyKpi_SummaryReportDto---
    public static KeyKpi_SummaryReportDto MapToDto(this KeyKpi_SummaryReport_ViewModel model)
    {
        return new KeyKpi_SummaryReportDto
        {
            SummaryReportItems = model.SummaryReportItems.Select(i => i.MapToDto()).ToList(),
            FinalScore = model.FinalScore,
        };
    }

    public static KeyKpi_SummaryReportItemDto MapToDto(this KeyKpi_SummaryReportItem_ViewModel model)
    {
        return new KeyKpi_SummaryReportItemDto
        {
            AverageScore = model.AverageScore,
            KeyIssueDepartment = model.KeyIssueDepartment.MapToDto(),
            KeyMetric = model.KeyMetric.MapToDto(),
            PeriodName = model.PeriodName,
            ReceivedScore = model.ReceivedScore,
            ReceivedSubmissions = model.ReceivedSubmissions,
            Submissions = model.Submissions.Select(s => s.MapToDto()).ToList(),
        };
    }

    public static KeyKpi_SummaryReport_SubmissionDto MapToDto(this KeyKpi_SummaryReport_Submission_ViewModel model)
    {
        return new KeyKpi_SummaryReport_SubmissionDto
        {
            CandidateDepartment = model.CandidateDepartment.MapToDto(),
            TotalScoreByCandidateDepartment = model.TotalScoreByCandidateDepartment,
            SubmissionDetails = model.SubmissionDetails.Select(d => d.MaptToDto()).ToList()
        };
    }

    public static KeyKpi_SummaryReport_SubmissionDetailDto MaptToDto(this KeyKpi_SummaryReport_SubmissionDetail_ViewModel model)
    {
        return new KeyKpi_SummaryReport_SubmissionDetailDto
        {
            CandidateGroup = model.CandidateGroup,
            CandidateName = model.CandidateName,
            CandidateUserCode = model.CandidateUserCode,
            CandidateUserName = model.CandidateUserName,
        };
    }
    // ---UserGroupViewModel to UserGroupDto---
    // public static UserGroupDto MapToDto_(this UserGroupViewModel model)
    // {

    // }

}
