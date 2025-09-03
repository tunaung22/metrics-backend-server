using Metrics.Application.Domains;
using Metrics.Application.DTOs.UserGroup;

namespace Metrics.Application.Common.Mappers;

public static class UserGroupMapper
{
    public static UserGroupDto MapToDto(this UserTitle e)
    {
        return new UserGroupDto(
            Id: e.Id,
            GroupCode: e.TitleCode,
            GroupName: e.TitleName
        );
    }

    // TODO: Mapt to Entity??
}
