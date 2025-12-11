using Metrics.Application.Domains;
using Metrics.Application.DTOs;
using Metrics.Application.DTOs.AccountDtos;
using Metrics.Application.DTOs.User;
using Metrics.Application.DTOs.UserClaims;
using Metrics.Application.Results;
using Microsoft.AspNetCore.Identity;

namespace Metrics.Application.Interfaces.IServices;

public interface IUserService
{

    Task<IdentityResult> RegisterUserAsync(UserCreateDto createDto);
    Task<ApplicationUser> UpdateAsync(string userId, UserUpdateDto updateDto);
    Task<ApplicationUser> UpdateProfileAsync(string userId, UserProfileUpdateDto updateDto);
    Task<IdentityResult> UpdatePasswordAsync(ApplicationUser user, string oldPassword, string newPassword);
    Task<ApplicationUser> FindByUsernameAsync(string username);
    Task<ApplicationUser?> FindByIdAsync(string userId);

    Task<long> FindCountAsync();
    Task<long> FindCountByDepartmentAsync(long departmentId);
    // Task<long> FindCountByDepartmentExcludeGroupAsync(long departmentId, List<long> excludedGroupId);
    Task<ResultT<List<UserDto>>> FindByDepartmentAsync(long departmentId);


    // TODOs
    // Task<long> FindByUserIdAsync(string userId);
    // lock user
    // Task<IdentityResult> LockUserByIdAsync(string userId);
    // unlock user
    // Task<IdentityResult> UnlockUserByIdAsync(string userId);
    // toggle lock user
    // Task<IdentityResult> ToggleLockUserByIdAsync(string userId);
    Task<IEnumerable<ApplicationUser>> FindAllActiveAsync(string roleName);
    Task<IEnumerable<ApplicationUser>> FindAllAsync();

    Task<ResultT<List<UserDto>>> FindAllAsync(int pageNumber = 1, int pageSize = 50);
    Task<ResultT<List<UserDto>>> FindAllAsync(
        string? searchTerm,
        int pageNumber = 1,
        int pageSize = 50);
    Task<ResultT<List<UserClaimDto>>> GetUserClaimsAsync(List<string> userIDs);
    // Task<ResultT<List<UserClaimDto>>> GetUserClaimsAsync(string userId);




    // ========== Return Entity ================================================
    // Task<ApplicationUser> CreateAsync(ApplicationUser entity);
    // Task<ApplicationUser> UpdateAsync(string username, ApplicationUser entity);
    // Task<bool> DeleteAsync(string username);
    // Task<ApplicationUser?> FindByUserCodeAsync(string username);
    // Task<IEnumerable<ApplicationUser>> FindAllAsync();


    Task<ResultT<UserDto>> FindByIdAsync_2(string userId);



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
