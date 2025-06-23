using Metrics.Application.Domains;
using Metrics.Application.DTOs.AccountDtos;
using Metrics.Application.DTOs.UserAccountDtos;
using Microsoft.AspNetCore.Identity;

namespace Metrics.Application.Interfaces.IServices;

public interface IUserService
{

    Task<IdentityResult> RegisterUserAsync(UserAccountCreateDto createDto);
    Task<ApplicationUser> UpdateAsync(string userId, UserUpdateDto updateDto);
    Task<ApplicationUser> UpdateProfileAsync(string userId, UserProfileUpdateDto updateDto);
    Task<ApplicationUser> FindByUsernameAsync(string username);
    Task<ApplicationUser?> FindByIdAsync(string userId);
    // TODOs
    // Task<long> FindByUserIdAsync(string userId);
    // lock user
    // Task<IdentityResult> LockUserByIdAsync(string userId);
    // unlock user
    // Task<IdentityResult> UnlockUserByIdAsync(string userId);
    // toggle lock user
    // Task<IdentityResult> ToggleLockUserByIdAsync(string userId);
    Task<IEnumerable<ApplicationUser>> FindAllActiveAsync();
    Task<IEnumerable<ApplicationUser>> FindAllAsync();


    // ========== Return Entity ================================================
    // Task<ApplicationUser> CreateAsync(ApplicationUser entity);
    // Task<ApplicationUser> UpdateAsync(string username, ApplicationUser entity);
    // Task<bool> DeleteAsync(string username);
    // Task<ApplicationUser?> FindByUserCodeAsync(string username);
    // Task<IEnumerable<ApplicationUser>> FindAllAsync();





    // Task<ApplicationUser> FindByIdAsync(long id);
    // ========== Return DTO ===================================================
    // Task<EmployeeGetDto> CreateAsync(EmployeeCreateDto createDto);
    // Task<EmployeeGetDto> UpdateAsync(string employeeCode, EmployeeUpdateDto updateDto);
    // Task<bool> DeleteAsync(string employeeCode);
    // Task<EmployeeGetDto> FindByIdAsync(long id);
    // // Task<EmployeeGetDto?> GetByIdAsync(long id);
    // Task<EmployeeGetDto> FindByEmployeeCodeAsync(string employeeCode);
    // // Task<IEnumerable<Department>> FindAsync(string keyword); // Search feature
    // Task<IEnumerable<EmployeeGetAllDto>> FindAllAsync();
    // Task<long> FindByUserIdAsync(string userId);


    // **TODO
    /*
    // Task<IEnumerable<Department>> FindAsync(string keyword); // Search feature
     Task<EmployeeGetDto> UpdateEmployeeCode(EmployeeUpdateDto updateDto);
     Task<bool> DeleteAsync(string employeeCode);
     Task<IEnumerable<EmployeeGetDto>> SearchEmployeeAsync(string keyword);
    */
}
