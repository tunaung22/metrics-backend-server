﻿namespace Metrics.Application.DTOs.DepartmentDtos;

public class DepartmentUpdateDto
{
    //public long Id { get; set; }
    // public Guid DepartmentCode { get; set; }
    public string DepartmentName { get; set; } = null!;

    // Collection Navigational Properties
    //public List<Employee> Employees { get; set; } = [];
    //public List<KpiSubmission> KpiSubmissions { get; set; } = [];
}
