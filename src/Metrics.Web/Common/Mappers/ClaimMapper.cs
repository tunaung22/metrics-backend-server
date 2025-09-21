using Metrics.Application.DTOs.UserClaims;
using Metrics.Web.Models.UserClaims;

namespace Metrics.Web.Common.Mappers;

public static class ClaimMapper
{
    public static ClaimDto MapToDto(this ClaimViewModel model)
    {
        return new ClaimDto
        {
            ClaimType = model.ClaimType,
            ClaimValue = model.ClaimValue,
        };
    }

    public static ClaimViewModel MapToViewModel(this ClaimDto dto)
    {
        return new ClaimViewModel
        {
            ClaimType = dto.ClaimType,
            ClaimValue = dto.ClaimValue
        };
    }
}
