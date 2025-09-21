using Metrics.Application.Domains;
using Metrics.Application.Results;

namespace Metrics.Application.Interfaces.IServices;

public interface IIdentityService
{
    // Task<ResultT<bool>> IsUserInRole(ApplicationUser user, List<ApplicationRole> userRoles);
    Task<ResultT<bool>> IsUserInTitle(ApplicationUser user, List<string> userTitles);



    // Task<ResultT<bool>> IsUserInTitle(string userId, List<UserTitle> userTitles);
}
