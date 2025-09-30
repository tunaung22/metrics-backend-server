namespace Metrics.Application.DTOs.UserGroup;

public record UserGroupDto(
    long Id,
    Guid GroupCode,
    string GroupName
    )
{ }
