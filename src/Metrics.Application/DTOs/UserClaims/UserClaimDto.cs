using Metrics.Application.DTOs.User;

namespace Metrics.Application.DTOs.UserClaims;

public record UserClaimDto
{
    public UserDto UserDto { get; set; } = null!;
    public List<ClaimDto> UserClaims { get; set; } = [];
}
