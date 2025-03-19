using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Domain.Entities;

public class KpiSubmission
{
    public long Id { get; set; }
    public DateTimeOffset SubmissionTime { get; set; }
    public decimal KpiScore { get; set; }
    public string? Comments { get; set; }
    public DateOnly SubmissionDate { get; set; } // Generated Field

    // Foreign Keys
    public long KpiPeriodId { get; set; }
    public long DepartmentId { get; set; }
    public long EmployeeId { get; set; }

    // Reference Navigational Properties
    public KpiPeriod KpiPeriod { get; set; } = null!;
    public Department TargetDepartment { get; set; } = null!;
    public Employee Candidate { get; set; } = null!;
}
