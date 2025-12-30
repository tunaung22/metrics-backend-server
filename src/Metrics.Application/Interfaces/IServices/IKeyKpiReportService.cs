using Metrics.Application.DTOs.Reports.KeyKpiSubmissionReports;
using Metrics.Application.DTOs.UserGroup;

namespace Metrics.Application.Interfaces.IServices;

public interface IKeyKpiReportService
{
    Task<MemoryStream> ExportExcel_KeyKpiSummaryReport(
        List<UserGroupDto> userGroupDtoList,
        List<KeyKpi_SummaryReportDto> summaryReportDto);
}
