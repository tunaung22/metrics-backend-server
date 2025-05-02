using Metrics.Application.Domains;

namespace Metrics.Application.Interfaces.IServices;

public interface IEmployeeService
{
    // ========== Return Entity ================================================
    Task<Employee> CreateAsync(Employee entity);
    Task<Employee> UpdateAsync(string employeeCode, Employee entity);
    Task<bool> DeleteAsync(string employeeCode);
    Task<Employee> FindByIdAsync(long id);
    Task<Employee?> FindByEmployeeCodeAsync(string employeeCode);
    Task<IEnumerable<Employee>> FindAllAsync();
    Task<long> FindByUserIdAsync(string userId);


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
