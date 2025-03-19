using Metrics.Application.DTOs;
using Metrics.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;

namespace Metrics.Application.Services.IServices;

public interface IUserAccountService
{

    Task<IdentityResult> RegisterUserAsync(UserAccountCreateDto dto);

}
