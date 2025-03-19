using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Domain.Entities;

public class Department
{
    public long Id { get; set; }

    public Guid DepartmentCode { get; set; }
    public string DepartmentName { get; set; } = null!;

    // Collection Navigational Properties
    public List<Employee> Employees { get; set; } = [];
    public List<KpiSubmission> KpiSubmissions { get; set; } = [];
}
