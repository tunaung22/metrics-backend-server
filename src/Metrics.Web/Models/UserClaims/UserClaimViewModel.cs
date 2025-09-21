
namespace Metrics.Web.Models.UserClaims;

public class UserClaimViewModel
{
    public required UserViewModel User { get; set; }
    public List<ClaimViewModel> Claims { get; set; } = [];

}
