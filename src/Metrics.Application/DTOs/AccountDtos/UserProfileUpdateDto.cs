using System.ComponentModel.DataAnnotations;

namespace Metrics.Application.DTOs.AccountDtos;

public class UserProfileUpdateDto
{
    [Required]
    public string Email { get; set; } = null!;
    [Required]
    public string FullName { get; set; } = null!;
    [Required]
    public string? ContactAddress { get; set; } = string.Empty;
    [Required]
    public string? PhoneNumber { get; set; } = string.Empty;

}
