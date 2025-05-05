using Metrics.Application.Interfaces;

namespace Metrics.Application.Domains;

public class Employee : IAuditColumn
{
    public long Id { get; set; }
    public string EmployeeCode { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ModifiedAt { get; set; } = DateTimeOffset.UtcNow;

    // ----- Foreign Keys
    public long DepartmentId { get; set; }
    public string ApplicationUserId { get; set; } = null!;

    // ----- Reference Navigational Properties  
    public Department Department { get; set; } = null!;
    public ApplicationUser ApplicationUser { get; set; } = null!;


    // ----- Collection Navigational Properties
    public List<KpiSubmission> KpiSubmissions { get; set; } = [];


    // public Employee(string employeeCode, string fullName, string? address, string? phoneNumber)
    // {
    //     EmployeeCode = employeeCode;
    //     FullName = fullName;
    //     Address = address;
    //     PhoneNumber = phoneNumber;
    // }
}
