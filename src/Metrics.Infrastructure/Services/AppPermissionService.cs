using Metrics.Application.Common.Mappers;
using Metrics.Application.DTOs.AppPermission;
using Metrics.Application.Interfaces.IServices;
using Metrics.Application.Results;
using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Metrics.Infrastructure.Services;

public class AppPermissionService(
    ILogger<AppPermissionService> logger,
    MetricsDbContext context) : IAppPermissionService
{
    private readonly ILogger<AppPermissionService> _logger = logger;
    private readonly MetricsDbContext _context = context;


    public async Task<ResultT<List<AppPermissionDto>>> FindAllPermissionsAsync()
    {
        try
        {
            var data = await _context.ApplicationPermissions
                .ToListAsync();
            var result = data.Select(p => p.MapToDto()).ToList();
            return ResultT<List<AppPermissionDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to fetch Application Permissions. {msg}", ex.Message);
            return ResultT<List<AppPermissionDto>>.Fail("Failed to fetch Application Permissions.", ErrorType.UnexpectedError);
        }
    }
}
