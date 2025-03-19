using Metrics.Application.DTOs.EmployeeDtos;
using Metrics.Domain.Entities;
using System;

namespace Metrics.Application.Services.IServices;

public interface IEmployeeService
{
    Task<EmployeeGetDto> FindByEmployeeCode_Async(string employeeCode);
    // Task<IEnumerable<Department>> FindAsync(string keyword); // Search feature
    Task<EmployeeGetDto> FindById_Async(long id);
    Task<EmployeeGetDto> Create_Async(EmployeeCreateDto createDto);
    Task<EmployeeGetDto> Update_Async(string employeeCode, EmployeeUpdateDto updateDto);
    Task<bool> Delete_Async(string employeeCode);
    Task<IEnumerable<EmployeeGetDto>> FindAll_Async();
    Task<IEnumerable<EmployeeGetAllDto>> FindAll2_Async();


    // Task<Employee?> GetByEmployeeCodeAsync(string employeeCode);
    // Task<EmployeeGetDto?> GetByIdAsync(long id);
    // Task<IEnumerable<EmployeeGetDto>> GetAllAsync();
    // Task<EmployeeGetDto> CreateAsync(EmployeeCreateDto createDto);
    // Task<EmployeeGetDto> UpdateAsync(EmployeeUpdateDto updateDto);
    // // TODO
    // // Task<EmployeeGetDto> UpdateEmployeeCode(EmployeeUpdateDto updateDto);
    // Task<bool> DeleteAsync(string employeeCode);
    // Task<IEnumerable<EmployeeGetDto>> SearchEmployeeAsync(string keyword);

}
