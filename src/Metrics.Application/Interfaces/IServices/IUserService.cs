using Metrics.Application.Domains;
using Metrics.Application.DTOs.UserAccountDtos;
using Microsoft.AspNetCore.Identity;

namespace Metrics.Application.Interfaces.IServices;

public interface IUserService
{

    Task<IdentityResult> RegisterUserAsync(UserAccountCreateDto createDto);
    Task<ApplicationUser> FindByUsernameAsync(string username);
    Task<ApplicationUser> FindByIdAsync(string userId);
    // TODOs
    // lock user
    // unlock user
    // reset password

    // Task<long> FindByUserIdAsync(string userId);

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
