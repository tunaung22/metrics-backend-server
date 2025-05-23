namespace Metrics.Application.DTOs.SeedingDtos;

public class SeedUserCreateDto
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string UserCode { get; set; }
    public required string UserTitleName { get; set; }
    public required List<string> RolesList { get; set; }
    public required string FullName { get; set; }
    public required string DepartmentName { get; set; }
    public string? ContactAddress { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; } = string.Empty;
}