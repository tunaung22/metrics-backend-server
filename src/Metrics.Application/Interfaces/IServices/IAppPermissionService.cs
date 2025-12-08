using Metrics.Application.DTOs.AppPermission;
using Metrics.Application.Results;

namespace Metrics.Application.Interfaces.IServices;

public interface IAppPermissionService
{
    Task<ResultT<List<AppPermissionDto>>> FindAllPermissionsAsync();
}

