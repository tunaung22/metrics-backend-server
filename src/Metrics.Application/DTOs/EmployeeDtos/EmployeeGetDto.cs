namespace Metrics.Application.DTOs.EmployeeDtos;

public class EmployeeGetDto
{
    public string EmployeeCode { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public long DepartmentId { get; set; }
    public string ApplicationUserId { get; set; } = null!;
}
