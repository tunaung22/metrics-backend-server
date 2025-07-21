
namespace Metrics.Web.Models;

public class UserGroupViewModel
{
    public long Id { get; set; }
    public Guid GroupCode { get; set; }
    public string GroupName { get; set; } = null!;
    public string? Description { get; set; } = string.Empty;
}
