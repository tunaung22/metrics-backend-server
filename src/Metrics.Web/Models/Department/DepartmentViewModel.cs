using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Web.Models.Department;

public class DepartmentViewModel
{
    public long Id { get; set; }

    [Required]
    // [StringLength(100)]
    public Guid DepartmentCode { get; set; }

    [Required]
    [StringLength(200)]
    public string DepartmentName { get; set; } = null!;

    // UI-specific properties
    // public bool IsSelected { get; set; }
    // ...
}
