using System;

namespace Metrics.Application.DTOs.KpiSubmissionDtos;

public class KpiSubmissionGetDto
{
    // public long Id { get; set; }
    public DateTimeOffset SubmissionTime { get; set; }
    public decimal KpiScore { get; set; }
    public string? Comments { get; set; }
    public DateOnly SubmissionDate { get; set; } // Generated Field

    // Foreign Keys
    // public long KpiPeriodId { get; set; }
    public string KpiPeriod { get; set; } = null!;
    // public long DepartmentId { get; set; }
    public string Department { get; set; } = null!;
    // public long EmployeeId { get; set; }
    public string Candidate { get; set; } = null!;

    // Reference Navigational Properties
    // public KpiPeriod KpiPeriod { get; set; } = null!;
    // public Department TargetDepartment { get; set; } = null!;
    // public Employee Candidate { get; set; } = null!;

}
