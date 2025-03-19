using Metrics.Domain.Entities;
using System;

namespace Metrics.Application.DTOs.EmployeeDtos;

public class EmployeeGetAllDto
{
    public string EmployeeCode { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public long DepartmentId { get; set; }
    public string ApplicationUserId { get; set; } = null!;
    public Department CurrentDepartment { get; set; } = null!;
    public ApplicationUser UserAccount { get; set; } = null!;


    // ----- Collection Navigational Properties
    public List<KpiSubmission> KpiSubmissions { get; set; } = [];
}
