using System;
using System.ComponentModel.DataAnnotations;

namespace Metrics.Web.ViewModels.DepartmentViewModels;

public class DepartmentFormViewModel
{
    [Required]
    [StringLength(200)]
    public string DepartmentName { get; set; } = null!;
}
