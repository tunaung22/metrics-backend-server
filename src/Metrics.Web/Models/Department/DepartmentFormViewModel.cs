using System;
using System.ComponentModel.DataAnnotations;

namespace Metrics.Web.Models.Department;

public class DepartmentFormViewModel
{
    [Required]
    [StringLength(200)]
    public string DepartmentName { get; set; } = null!;
}
