using Metrics.Application.DTOs.UserClaims;
using System.Security.Claims;

namespace Metrics.Application.Common.Mappers;

public static class ClaimMapper
{
    public static ClaimDto MapToDto(this Claim claim)
    {
        return new ClaimDto
        {
            ClaimType = claim.Type,
            ClaimValue = claim.Value
        };
    }
}
