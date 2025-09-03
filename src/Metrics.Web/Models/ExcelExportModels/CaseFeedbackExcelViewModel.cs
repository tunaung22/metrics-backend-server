using MiniExcelLibs.Attributes;

namespace Metrics.Web.Models.ExcelExportModels;

public class CaseFeedbackExcelViewModel
{
    [ExcelColumnWidth(14)]
    [ExcelColumn(Name = "Period Name")]
    public string? PeriodName { get; set; }

    [ExcelColumnWidth(20)]
    [ExcelColumn(Name = "Submitted By")]
    public string? SubmittedBy { get; set; }

    [ExcelColumnWidth(20)]
    [ExcelColumn(Name = "Case Department")]
    public string CaseDepartment { get; set; } = null!;

    [ExcelColumnWidth(20)]
    [ExcelColumn(Name = "Incident Time")]
    public DateTime IncidentTime { get; set; }

    [ExcelColumnWidth(14)]
    [ExcelColumn(Name = "Given Score")]
    public decimal Score { get; set; }

    [ExcelColumnWidth(20)]
    [ExcelColumn(Name = "Ward Name")]
    public string WardName { get; set; } = null!;

    [ExcelColumnWidth(20)]
    [ExcelColumn(Name = "CPI Number")]
    public string CPINumber { get; set; } = null!;

    [ExcelColumnWidth(20)]
    [ExcelColumn(Name = "Patient Name")]
    public string PatientName { get; set; } = null!;

    [ExcelColumnWidth(15)]
    [ExcelColumn(Name = "Room Number")]
    public string RoomNumber { get; set; } = null!;

    [ExcelColumnWidth(30)]
    [ExcelColumn(Name = "Case Details")]
    public string? Description { get; set; } = string.Empty;

    [ExcelColumnWidth(30)]
    [ExcelColumn(Name = "Suggestions")]
    public string? Comments { get; set; } = string.Empty;
}