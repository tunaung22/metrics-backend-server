using Metrics.Application.DTOs.Department;
using Metrics.Application.DTOs.KpiPeriod;
using Metrics.Application.DTOs.User;

namespace Metrics.Application.DTOs.KpiSubmissionDtos;

public class KpiSubmissionDto
{
    // public long Id { get; set; }
    public DateTimeOffset SubmissionTime { get; set; }
    public DateOnly SubmissionDate { get; set; } // Generated Field
    public decimal ScoreValue { get; set; }
    public string? Comments { get; set; }
    public string? PositiveAspects { get; set; }
    public string? NegativeAspects { get; set; }


    // Foreign Keys
    public long KpiPeriodId { get; set; }
    public long DepartmentId { get; set; }
    public DepartmentDto TargetDepartment { get; set; } = null!;
    public string SubmitterId { get; set; } = null!;
    public UserDto SubmittedBy { get; set; } = null!;

    // Reference Navigational Properties
    public KpiPeriodDto KpiPeriod { get; set; } = null!;
    // public Department TargetDepartment { get; set; } = null!;

}
