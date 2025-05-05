using Metrics.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Metrics.Application.Domains;

public class ApplicationRole : IdentityRole, IAuditColumn
{
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ModifiedAt { get; set; } = DateTimeOffset.UtcNow;
}
