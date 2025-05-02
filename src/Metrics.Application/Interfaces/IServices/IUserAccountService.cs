using Metrics.Application.DTOs.UserAccountDtos;
using Microsoft.AspNetCore.Identity;

namespace Metrics.Application.Interfaces.IServices;

public interface IUserAccountService
{

    Task<IdentityResult> RegisterUserAsync(UserAccountCreateDto dto);
}
