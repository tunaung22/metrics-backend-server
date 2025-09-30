namespace Metrics.Application.DTOs.UserClaims;

public record ClaimDto
{
    public string ClaimType { get; set; } = null!;
    public string ClaimValue { get; set; } = null!;
}
