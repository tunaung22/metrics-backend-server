using Metrics.Application.DTOs.User;
using Microsoft.AspNetCore.Identity;

namespace Metrics.Application.Interfaces.IServices;

public interface IUserAccountService
{

    Task<IdentityResult> RegisterUserAsync(UserCreateDto dto);
}
