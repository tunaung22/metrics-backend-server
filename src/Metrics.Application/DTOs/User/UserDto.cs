using Metrics.Application.DTOs.Department;
using Metrics.Application.DTOs.UserGroup;

namespace Metrics.Application.DTOs.User;

public record UserDto(
    string Id,
    string UserName,
    string FullName,
    string PhoneNumber,
    string ContactAddress,
    UserGroupDto UserGroup,
    DepartmentDto Department
)
{ }
