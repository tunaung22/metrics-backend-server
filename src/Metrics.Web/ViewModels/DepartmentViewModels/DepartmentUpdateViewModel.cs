using System;
using System.ComponentModel.DataAnnotations;

namespace Metrics.Web.ViewModels.DepartmentViewModels;

public class DepartmentUpdateViewModel
{
    public long Id { get; set; }

    public Guid DepartmentCode { get; set; }

    [Required(ErrorMessage = "Department name is required.")]
    [StringLength(200)]
    public string DepartmentName { get; set; } = null!;
}
