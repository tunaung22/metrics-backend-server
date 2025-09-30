using Metrics.Application.DTOs.User;

namespace Metrics.Application.DTOs.Department;

public class DepartmentDto
{

    public long Id { get; set; }
    public Guid DepartmentCode { get; set; }
    public string DepartmentName { get; set; } = null!;

    public List<UserDto> DepartmentStaffs { get; set; } = [];
    public int StaffCount { get; set; }
    // Collection Navigational Properties
    //public List<KpiSubmission> KpiSubmissions { get; set; } = [];
}
