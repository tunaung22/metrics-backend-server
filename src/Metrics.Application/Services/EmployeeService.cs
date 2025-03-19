using Metrics.Application.DTOs.EmployeeDtos;
using Metrics.Application.Mappers.DtoMappers;
using Metrics.Application.Services.IServices;
using Metrics.Domain.Entities;
using Metrics.Domain.Exceptions;
using Metrics.Infrastructure;
using Metrics.Infrastructure.Data;
using Metrics.Infrastructure.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;

namespace Metrics.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly MetricsDbContext _context;
    private readonly ILogger<EmployeeService> _logger;
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeService(
        MetricsDbContext context,
        ILogger<EmployeeService> logger,
        IEmployeeRepository employeeRepository)
    {
        _context = context;
        _logger = logger;
        _employeeRepository = employeeRepository;
    }

    public async Task<EmployeeGetDto> Create_Async(EmployeeCreateDto createDto)
    {
        try
        {
            if (await _employeeRepository.EmployeeExistsAsync(createDto.EmployeeCode.ToString()))
                // TODO: DuplicateEntityException
                throw new DuplicateContentException("Employee code already exist.");

            var entity = createDto.ToEntity();
            _employeeRepository.Create(entity);
            await _context.SaveChangesAsync();

            return createDto.ToGetDto();
        }
        catch (DbUpdateException ex)
        {
            // when (ex.InnerException is PostgresException postgresEx && postgresEx.SqlState == "23505")
            if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                _logger.LogError(ex, pgEx.MessageText);
                // TODO: DuplicateEntityException
                throw new DuplicateContentException("Employee already exist.", ex.InnerException);
            }
            else
            {
                // Handle database-specific errors
                _logger.LogError(ex, "Database error while creating department.");
                // TODO: DatabaseException
                throw new Exception("A database error occurred.");
            }
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while creating department.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<bool> Delete_Async(string employeeCode)
    {
        try
        {
            var employee = await _employeeRepository.FindByEmployeeCodeAsync(employeeCode);
            if (employee == null)
                return true; // Idempotent: Treat as success

            if (employee == null)
                // TODO: NotFoundException
                throw new Exception("Employee not found.");

            _employeeRepository.Delete(employee);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (DbUpdateException ex)
        {
            // Handle database-specific errors
            _logger.LogError(ex, "Database error while deleting employee.");
            // TODO: DatabaseException
            throw new Exception("A database error occurred.");
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while deleting employee.");
            // throw; // Propagate to global exception handler
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<IEnumerable<EmployeeGetDto>> FindAll_Async()
    {
        try
        {
            var employees = await _employeeRepository.FindAllAsync();

            return employees.ToGetDto();
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while querying employees.");
            // throw; // Propagate to global exception handler
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<IEnumerable<EmployeeGetAllDto>> FindAll2_Async()
    {
        try
        {
            var employees = await _employeeRepository.FindAllAsQueryable()
                .Include(e => e.CurrentDepartment)
                // .Include(e => e.UserAccount)
                .Include(e => e.KpiSubmissions)
                .ToListAsync();

            return employees.Select(e => new EmployeeGetAllDto
            {
                EmployeeCode = e.EmployeeCode,
                FullName = e.FullName,
                Address = e.Address,
                PhoneNumber = e.PhoneNumber,
                DepartmentId = e.DepartmentId,
                CurrentDepartment = e.CurrentDepartment,
                ApplicationUserId = e.ApplicationUserId
            }).ToList();
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while querying employees.");
            // throw; // Propagate to global exception handler
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<EmployeeGetDto> FindByEmployeeCode_Async(string employeeCode)
    {
        try
        {
            if (string.IsNullOrEmpty(employeeCode))
                // TODO: ValidationException
                throw new Exception("Parameter employeeCode is required.");

            var employee = await _employeeRepository.FindByEmployeeCodeAsync(employeeCode);
            if (employee == null)
                // TODO: NotFoundException
                throw new Exception($"Employee with code {employeeCode} not found.");

            return employee.ToGetDto();
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while querying department by department code.");
            // throw; // Propagate to global exception handler
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<EmployeeGetDto> FindById_Async(long id)
    {
        try
        {
            if (id <= 0)
                // TODO: ValidationException
                throw new Exception("Parameter id is required.");

            var employee = await _employeeRepository.FindByIdAsync(id);
            if (employee == null)
                // TODO: NotFoundException
                throw new Exception($"Employee with id {id} not found.");

            return employee.ToGetDto();
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while querying department by department code.");
            // throw; // Propagate to global exception handler
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<EmployeeGetDto> Update_Async(string employeeCode, EmployeeUpdateDto updateDto)
    {
        try
        {
            // Validate existence
            var targetEmployee = await _employeeRepository.FindByEmployeeCodeAsync(employeeCode);
            if (targetEmployee == null)
                // TODO: NotFoundException
                throw new NotFoundException("Employee not found.");

            // Handle concurrency (example using row version)
            // if (existing.RowVersion != department.RowVersion)
            //     return Result<Department>.Fail("Concurrency conflict.");

            // Track changes

            _employeeRepository.Update(targetEmployee);
            targetEmployee.FullName = updateDto.FullName;
            targetEmployee.PhoneNumber = updateDto.PhoneNumber;
            targetEmployee.Address = updateDto.Address;
            targetEmployee.DepartmentId = updateDto.DepartmentId;

            await _context.SaveChangesAsync();

            return targetEmployee.ToGetDto();
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                _logger.LogError(ex, pgEx.MessageText);
                // TODO: DuplicateEntityException
                throw new Exception("Duplicate entry exception occurred.");
            }
            else
            {
                // Handle database-specific errors
                _logger.LogError(ex, "Database error while updating employee.");
                // TODO: DatabaseException
                throw new Exception("A database error occurred.");
            }
            // Handle database-specific errors
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while updating employee.");
            throw new Exception("An unexpected error occurred. Please try again later.");

        }
    }
}
// private readonly IUnitOfWork _unitOfWork;

// public EmployeeService(IUnitOfWork unitOfWork)
// {
//     _unitOfWork = unitOfWork;
// }


// // =========================================================================
// public async Task<EmployeeGetDto> CreateAsync(EmployeeCreateDto createDto)
// {
//     if (await _unitOfWork.EmployeeRepository.EmployeeExistAsync(createDto.EmployeeCode.ToString()))
//         throw new Exception($"Employee code {createDto.EmployeeCode} already exist.");

//     var entity = createDto.ToEntity();
//     await _unitOfWork.EmployeeRepository.CreateAsync(entity);
//     if (await _unitOfWork.SaveChangesAsync() > 0)
//     {
//         return createDto.ToGetDto();
//     }

//     throw new Exception("Create new employee failed.");
// }

// public async Task<bool> DeleteAsync(string employeeCode)
// {
//     // var employee = await _unitOfWork.EmployeeRepository.EmployeeExistAsync(employeeCode);
//     // if (employee)
//     //     throw new Exception($"Employee with code {employeeCode} does not exist.");
//     var employeeEntity = await _unitOfWork.EmployeeRepository.FindByEmployeeCodeAsync(employeeCode);

//     // TODO: Catch EmployeeNotFoundException

//     _unitOfWork.EmployeeRepository.Delete(employeeEntity);

//     return await _unitOfWork.SaveChangesAsync() > 0;
// }

// public async Task<IEnumerable<EmployeeGetDto>> GetAllAsync()
// {
//     var employeeEntity = await _unitOfWork.EmployeeRepository.FindAllAsync();
//     var result = employeeEntity ?? [];

//     return result.ToGetDto();
// }

// public async Task<Employee?> GetByEmployeeCodeAsync(string employeeCode)
// {
//     var employee = await _unitOfWork.EmployeeRepository.FindByEmployeeCodeAsync(employeeCode);

//     // TODO: Catch EmployeeNotFoundException

//     return employee;
// }

// public Task<EmployeeGetDto?> GetByIdAsync(long id)
// {
//     throw new NotImplementedException();
// }

// public Task<IEnumerable<EmployeeGetDto>> SearchEmployeeAsync(string keyword)
// {
//     // call and get IQueryable to filter more then enumrate
//     throw new NotImplementedException();
// }

// public Task<EmployeeGetDto> UpdateAsync(EmployeeUpdateDto updateDto)
// {
//     throw new NotImplementedException();
// }
