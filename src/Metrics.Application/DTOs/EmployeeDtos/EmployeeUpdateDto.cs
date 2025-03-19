using System;

namespace Metrics.Application.DTOs.EmployeeDtos;

public class EmployeeUpdateDto
{
    // public string EmployeeCode { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public long DepartmentId { get; set; }
    // public string ApplicationUserId { get; set; } = null!;
}
