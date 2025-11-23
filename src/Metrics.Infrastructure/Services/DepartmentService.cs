using Metrics.Application.Common.Mappers;
using Metrics.Application.Domains;
using Metrics.Application.DTOs.Department;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Application.Interfaces.IServices;
using Metrics.Application.Results;
using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Metrics.Infrastructure.Services;

public class DepartmentService(
        MetricsDbContext context,
        // IUnitOfWork unitOfWork,
        ILogger<DepartmentService> logger,
        IDepartmentRepository departmentRepository) : IDepartmentService
{
    private readonly MetricsDbContext _context = context;
    // private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DepartmentService> _logger = logger;
    private readonly IDepartmentRepository _departmentRepository = departmentRepository;


    public async Task<ResultT<List<DepartmentDto>>> FindAllAsync(int pageNumber = 1, int pageSize = 50)
    {
        try
        {
            var data = await _departmentRepository.FindAllAsync(pageNumber, pageSize);
            var result = data.Select(e => e.MapToDto()).ToList();

            return ResultT<List<DepartmentDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occured while fetching all departments. {msg}", ex.Message);
            return ResultT<List<DepartmentDto>>.Fail("Failed to fetch departments.", ErrorType.UnexpectedError);
        }
    }

    public async Task<ResultT<DepartmentDto>> FindByCodeAsync(string departmentCode)
    {
        try
        {
            var department = await _departmentRepository.FindByDepartmentCodeAsync(departmentCode);
            if (department == null)
                return ResultT<DepartmentDto>.Fail("No department found.", ErrorType.NotFound);

            return ResultT<DepartmentDto>.Success(department.MapToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occured while fetching department. {msg}", ex.Message);
            return ResultT<DepartmentDto>.Fail("Failed to fetch department.", ErrorType.UnexpectedError);
        }
    }

    public async Task<Result> CreateAsync(DepartmentCreateDto createDto)
    {
        try
        {
            _departmentRepository.Create(createDto.MapToEntity());
            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            // when (ex.InnerException is PostgresException postgresEx && postgresEx.SqlState == "23505")
            if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                _logger.LogError(ex, "Database error {e}", pgEx.MessageText);
                return Result.Fail("Failed to create new department.", ErrorType.DuplicateKey);
            }
            else
            {
                _logger.LogError(ex, "Database error while creating department. {e}", ex.Message);
                return Result.Fail("Failed to create new department.", ErrorType.DatabaseError);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create new department. {msg}", ex.Message);
            return Result.Fail("Failed to create new department.", ErrorType.UnexpectedError);
        }
    }

    public async Task<Department> UpdateAsync(string departmentCode, Department department)
    {
        if (string.IsNullOrEmpty(departmentCode) || string.IsNullOrWhiteSpace(departmentCode))
            throw new ArgumentNullException("Department code is required.");

        // TODO: does this check necessary?
        if (departmentCode != department.DepartmentCode.ToString())
            throw new DbUpdateException("Update failed. Hint: departmentCode to update and existing departmentCode does not match.");

        try
        {
            var targetDepartment = await _departmentRepository.FindByDepartmentCodeAsync(departmentCode);
            if (targetDepartment == null)
                throw new NotFoundException("Department not found.");

            // Handle concurrency (example using row version)
            // if (existing.RowVersion != department.RowVersion)
            //     return Result<Department>.Fail("Concurrency conflict.");

            // Note: This is full update (**not partial update)
            _departmentRepository.Update(targetDepartment);
            targetDepartment.DepartmentCode = department.DepartmentCode;
            targetDepartment.DepartmentName = department.DepartmentName;
            await _context.SaveChangesAsync();

            // refetch updated entity
            var updatedEntity = await _departmentRepository.FindByDepartmentCodeAsync(departmentCode);
            if (updatedEntity == null)
                throw new NotFoundException("Department not found.");

            return updatedEntity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating department.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<bool> DeleteAsync(string departmentCode)
    {
        if (string.IsNullOrEmpty(departmentCode) || string.IsNullOrWhiteSpace(departmentCode))
            throw new ArgumentNullException("Department code is required.");

        try
        {
            var targetDepartment = await _departmentRepository.FindByDepartmentCodeAsync(departmentCode);
            if (targetDepartment == null)
                throw new NotFoundException("Department not found.");
            // or return true; // Idempotent: Treat as success

            _departmentRepository.Delete(targetDepartment);

            return await _context.SaveChangesAsync() > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting department.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<Department> FindByIdAsync(long id)
    {
        try
        {
            // var department = await _departmentRepository.FindByIdAsync(id);
            var department = await _departmentRepository.FindByIdAsync(id);
            if (department == null)
                throw new NotFoundException($"Department with id {id} not found.");

            return department;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying department by department code.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<Department> FindByDepartmentCodeAsync(string departmentCode)
    {
        try
        {
            var _departmentCode = departmentCode.Trim().ToLower();
            if (string.IsNullOrEmpty(_departmentCode) || string.IsNullOrWhiteSpace(_departmentCode))
                throw new ArgumentNullException("Parameter departmentCode is required.");

            var department = await _departmentRepository.FindByDepartmentCodeAsync(departmentCode);
            if (department == null)
                throw new NotFoundException($"Department with code {departmentCode} not found.");

            return department;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying department by department code.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }
    public async Task<Department?> FindByDepartmentNameAsync(string departmentName)
    {
        try
        {
            if (string.IsNullOrEmpty(departmentName) || string.IsNullOrWhiteSpace(departmentName))
                throw new ArgumentNullException("Parameter departmentCode is required.");

            var department = await _departmentRepository.FindByDepartmentNameAsync(departmentName);
            // if (department == null)
            //     throw new NotFoundException($"Department with code {departmentName} not found.");

            return department;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying department by department code.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<IEnumerable<Department>> FindAllAsync()
    {
        try
        {
            return await _departmentRepository.FindAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying departments.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<ResultT<List<DepartmentDto>>> FindAll_R_Async()
    {
        try
        {
            var data = await _departmentRepository.FindAllAsync();
            var result = data.Select(e => e.MapToDto()).ToList();

            return ResultT<List<DepartmentDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occured while fetching all departments. {msg}", ex.Message);
            return ResultT<List<DepartmentDto>>.Fail("Failed to fetch departments.", ErrorType.UnexpectedError);
        }
    }

    public async Task<IEnumerable<Department>> FindAll_Async(int pageNumber = 1, int pageSize = 20)
    {
        try
        {
            // var departments = await _departmentRepository.FindAllAsQueryable()
            //     .Skip((pageNumber - 1) * pageSize)
            //     .Take(pageSize)
            //     .ToListAsync();
            var departments = await _departmentRepository
                .FindAllAsync(pageNumber, pageSize);

            return departments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying departments.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<long> FindCountAsync()
    {
        try
        {
            return await _departmentRepository.FindCountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while counting departments.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }


}