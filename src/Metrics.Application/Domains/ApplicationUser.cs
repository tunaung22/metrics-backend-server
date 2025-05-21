using Metrics.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Metrics.Application.Domains;

public class ApplicationUser : IdentityUser, IAuditColumn
{
    public string UserCode { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string ContactAddress { get; set; } = string.Empty;
    public string ProfilePictureUrl { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ModifiedAt { get; set; } = DateTimeOffset.UtcNow;

    // ----- Foreign Keys
    public long DepartmentId { get; set; }
    public long UserTitleId { get; set; }

    // ----- Reference Navigational Properties
    public Department Department { get; set; } = null!;
    public UserTitle UserTitle { get; set; } = null!;

    // ----- Collection Navigational Properties
    public List<KpiSubmission> KpiSubmissions { get; set; } = [];
    public List<KeyKpiSubmission> KeyKpiSubmissions { get; set; } = [];
}
