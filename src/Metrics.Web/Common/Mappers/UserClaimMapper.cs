using Metrics.Application.DTOs.UserClaims;
using Metrics.Web.Models.UserClaims;

namespace Metrics.Web.Common.Mappers;

public static class UserClaimMapper
{
    // UserClaimViewModel to UserClaimDto


    // UserClaimDto to UserClaimViewModel
    public static UserClaimViewModel MapToViewModel(this UserClaimDto dto)
    {
        return new UserClaimViewModel
        {
            User = dto.UserDto.MapToViewModel(),
            Claims = dto.UserClaims.Select(uc => uc.MapToViewModel()).ToList()
        };
    }
}
