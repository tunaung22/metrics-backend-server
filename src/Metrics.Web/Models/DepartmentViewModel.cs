using System.ComponentModel.DataAnnotations;

namespace Metrics.Web.Models;

public class DepartmentViewModel
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Department Code is required.")]
    public Guid DepartmentCode { get; set; }

    [Required(ErrorMessage = "Department Name is required.")]
    [StringLength(200)]
    public required string DepartmentName { get; set; }
}