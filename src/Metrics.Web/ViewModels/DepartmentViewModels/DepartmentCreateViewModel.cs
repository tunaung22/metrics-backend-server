using System;
using System.ComponentModel.DataAnnotations;

namespace Metrics.Web.ViewModels.DepartmentViewModels;

public class DepartmentCreateViewModel : DepartmentViewModel
{
    // [Required]
    // // [StringLength(100)]
    // public Guid DepartmentCode { get; set; }

    [Required(ErrorMessage = "Department name is needed.")]
    [StringLength(200)]
    public new string DepartmentName { get; set; } = null!;

}