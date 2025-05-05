using Metrics.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Metrics.Application.Domains;

public class ApplicationUser : IdentityUser, IAuditColumn
{
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ModifiedAt { get; set; } = DateTimeOffset.UtcNow;
}
