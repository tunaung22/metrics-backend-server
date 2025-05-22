using Metrics.Application.Interfaces;

namespace Metrics.Application.Domains;

public class UserTitle : IAuditColumn, ISoftDelete
{
    public long Id { get; set; }
    public Guid TitleCode { get; set; }
    public string TitleName { get; set; } = null!;
    public string? Description { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ModifiedAt { get; set; } = DateTimeOffset.UtcNow;

    // ----- Collection Navigational Properties
    public List<ApplicationUser> ApplicationUsers { get; set; } = [];
}
