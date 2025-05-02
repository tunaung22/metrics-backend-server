namespace Metrics.Application.DTOs.SeedingDtos;

public class DefaultUserCreateDto
{
    public required string DefaultUsername { get; set; }
    public required string DefaultEmail { get; set; }
    public required string DefaultPassword { get; set; }
    public required string DefaultRoleName { get; set; }
    public required string DefaultFullName { get; set; }
    public required string DefaultDepartmentName { get; set; }
    public required string EmployeeCode { get; set; }
    public required string EmployeeFullName { get; set; }
    public string? EmployeeAddress { get; set; } = string.Empty;
    public string? EmployeePhoneNumber { get; set; } = string.Empty;
}
