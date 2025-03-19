using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Domain.Entities;

public class Employee
{
    public long Id { get; set; }
    public string EmployeeCode { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }

    // ----- Foreign Keys
    public long DepartmentId { get; set; }
    public string ApplicationUserId { get; set; } = null!;

    // ----- Reference Navigational Properties  
    public Department CurrentDepartment { get; set; } = null!;
    public ApplicationUser UserAccount { get; set; } = null!;


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
