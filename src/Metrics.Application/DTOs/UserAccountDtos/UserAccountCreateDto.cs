using System.ComponentModel.DataAnnotations;

namespace Metrics.Application.DTOs.UserAccountDtos;

public class UserAccountCreateDto
{
    [Required]
    public string UserName { get; set; } = null!; // Use username instead of email
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;
    [Required]
    public string EmployeeCode { get; set; } = null!;
    [Required]
    public string FullName { get; set; } = null!;
    public string? Address { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; } = string.Empty;
    public long DepartmentId { get; set; }
    // public string RoleId { get; set; } = string.Empty;
    public List<string> RoleIds { get; set; } = [];
    public string ApplicationUserId { get; set; } = string.Empty;
}
