using Metrics.Application.DTOs.UserGroup;
using Metrics.Web.Models;

namespace Metrics.Web.Common.Mappers;

public static class UserGroupMapper
{
    public static UserGroupDto MapToDto(this UserGroupViewModel model)
    {
        return new UserGroupDto(
            Id: model.Id,
            GroupCode: model.GroupCode,
            GroupName: model.GroupName
        );
    }

    public static UserGroupViewModel MapToViewModel(this UserGroupDto dto)
    {
        return new UserGroupViewModel
        {
            Id = dto.Id,
            GroupCode = dto.GroupCode,
            GroupName = dto.GroupName,
            Description = string.Empty
        };
    }

}
