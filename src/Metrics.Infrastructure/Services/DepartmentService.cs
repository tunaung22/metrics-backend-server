using Metrics.Application.Domains;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Application.Interfaces.IServices;
using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Metrics.Infrastructure.Services;

public class DepartmentService : IDepartmentService
{
    private readonly MetricsDbContext _context;
    // private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DepartmentService> _logger;
    private readonly IDepartmentRepository _departmentRepository;

    public DepartmentService(
        MetricsDbContext context,
        // IUnitOfWork unitOfWork,
        ILogger<DepartmentService> logger,
        IDepartmentRepository departmentRepository)
    {
        _context = context;
        // _unitOfWork = unitOfWork;
        _logger = logger;
        _departmentRepository = departmentRepository;
    }


    public async Task<Department> CreateAsync(Department department)
    {
        // if (await _departmentRepository.DepartmentExistsAsync(entity.DepartmentName.Trim()))
        //     throw new DuplicateContentException("Department name already exist.");

        try
        {
            _departmentRepository.Create(department);
            await _context.SaveChangesAsync();

            return department;
        }
        catch (DbUpdateException ex)
        {
            // when (ex.InnerException is PostgresException postgresEx && postgresEx.SqlState == "23505")
            if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                _logger.LogError(ex, pgEx.MessageText);
                // TODO: DuplicateEntityException
                throw new DuplicateContentException("Department already exist.", ex.InnerException);
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

    public async Task<IEnumerable<Department>> FindAllAsync(int pageNumber = 1, int pageSize = 20)
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


// ========== Operation Result Pattern Implmentation =======================
// public async Task<Result<Department>> FindById_ResultAsync(long id)
// {
//     try
//     {
//         var department = await _unitOfWork.DepartmentRepository.FindByIdAsync(id);

//         if (department == null)
//             return Result<Department>.Fail($"Department with id {id} not found.", ErrorType.NotFound);

//         return Result<Department>.Ok(department);
//     }
//     catch (Exception ex)
//     {
//         _logger.LogError(ex, "Unexpected error while querying department by department code.");
//         return Result<Department>.Fail("An unexpected error occurred. Please try again later.", ErrorType.UnexpectedError);
//     }
// }

// public async Task<Result<Department>> FindByDepartmentCode_ResultAsync(string departmentCode)
// {
//     try
//     {
//         var _departmentCode = departmentCode.Trim().ToLower();
//         if (string.IsNullOrEmpty(_departmentCode) || string.IsNullOrWhiteSpace(_departmentCode))
//             return Result<Department>.Fail("Parameter departmentCode is required.", ErrorType.InvalidArgument);

//         var department = await _unitOfWork.DepartmentRepository.FindByDepartmentCodeAsync(departmentCode);
//         if (department == null)
//             return Result<Department>.Fail($"Department with code {departmentCode} not found.", ErrorType.NotFound);

//         return Result<Department>.Ok(department);
//     }
//     catch (Exception ex)
//     {
//         _logger.LogError(ex, "Unexpected error while querying department by department code.");
//         return Result<Department>.Fail("An unexpected error occurred. Please try again later.", ErrorType.UnexpectedError);
//     }
// }

// public async Task<Result<Department>> Create_ResultAsync(Department entity)
// {
//     try
//     {
//         if (await _unitOfWork.DepartmentRepository.DepartmentExistsAsync(entity.DepartmentName))
//             return Result<Department>.Fail("Department already exist.", ErrorType.DuplicateKey);

//         _unitOfWork.DepartmentRepository.Create(entity);
//         var created = await _unitOfWork.SaveChangesAsync();

//         if (created > 0)
//             return Result<Department>.Ok(entity);

//         return Result<Department>.Fail("Create new department failed. Please try again later.", ErrorType.UnexpectedError);
//     }
//     catch (Exception ex)
//     {
//         _logger.LogError(ex, "Unexpected error while creating department.");
//         return Result<Department>.Fail("An unexpected error occurred. Please try again later.", ErrorType.UnexpectedError);
//     }
// }

// public async Task<Result<Department>> Update_ResultAsync(string departmentCode, Department entity)
// {
//     try
//     {
//         if (string.IsNullOrEmpty(departmentCode) || string.IsNullOrWhiteSpace(departmentCode))
//             return Result<Department>.Fail("Department code is required.", ErrorType.InvalidArgument);

//         var targetDepartment = await _unitOfWork.DepartmentRepository.FindByDepartmentCodeAsync(departmentCode);
//         if (targetDepartment == null)
//             return Result<Department>.Fail("Department not found.", ErrorType.NotFound);

//         // Handle concurrency (example using row version)
//         // if (existing.RowVersion != department.RowVersion)
//         //     return Result<Department>.Fail("Concurrency conflict.");

//         _unitOfWork.DepartmentRepository.Update(targetDepartment);
//         targetDepartment.DepartmentName = entity.DepartmentName;

//         await _unitOfWork.SaveChangesAsync();
//         var updatedEntity = await _unitOfWork.DepartmentRepository.FindByDepartmentCodeAsync(departmentCode);

//         return Result<Department>.Ok(updatedEntity);
//     }
//     catch (Exception ex)
//     {
//         _logger.LogError(ex, "Unexpected error while updating department.");
//         return Result<Department>.Fail("An unexpected error occurred. Please try again later.", ErrorType.UnexpectedError);
//     }
// }

// public async Task<Result<bool>> Delete_ResultAsync(string departmentCode)
// {
//     try
//     {
//         var department = await _unitOfWork.DepartmentRepository.FindByDepartmentCodeAsync(departmentCode);
//         if (department == null)
//             return Result<bool>.Ok(true); // Idempotent: Treat as success

//         if (department == null)
//             return Result<bool>.Fail("Department not found.", ErrorType.NotFound);

//         _unitOfWork.DepartmentRepository.Delete(department);
//         var deleted = await _unitOfWork.SaveChangesAsync();

//         if (deleted > 0)
//             return Result<bool>.Ok(true);

//         return Result<bool>.Fail("Create new department failed. Please try again later.", ErrorType.UnexpectedError);
//     }
//     catch (Exception ex)
//     {
//         _logger.LogError(ex, "Unexpected error while deleting department.");
//         return Result<bool>.Fail("An unexpected error occurred. Please try again later.", ErrorType.UnexpectedError);
//     }
// }

// public async Task<Result<IEnumerable<Department>>> FindAll_ResultAsync()
// {
//     try
//     {
//         var departments = await _unitOfWork.DepartmentRepository.FindAllAsync();

//         return Result<IEnumerable<Department>>.Ok(departments);
//     }
//     catch (Exception ex)
//     {
//         _logger.LogError(ex, "Unexpected error while querying departments.");
//         return Result<IEnumerable<Department>>.Fail("An unexpected error occurred. Please try again later.", ErrorType.UnexpectedError);
//     }
// }

// public async Task<Result<IEnumerable<Department>>> FindAll_ResultAsync(int pageNumber = 1, int pageSize = 20)
// {
//     try
//     {
//         var departments = await _unitOfWork.DepartmentRepository.FindAllAsQueryable()
//             .Skip((pageNumber - 1) * pageSize)
//             .Take(pageSize)
//             .ToListAsync();

//         return Result<IEnumerable<Department>>.Ok(departments);
//     }
//     catch (Exception ex)
//     {
//         _logger.LogError(ex, "Unexpected error while querying departments.");
//         return Result<IEnumerable<Department>>.Fail("An unexpected error occurred. Please try again later.", ErrorType.UnexpectedError);
//     }
// }

// public async Task<Result<long>> FindCount_ResultAsync()
// {
//     try
//     {
//         var count = await _unitOfWork.DepartmentRepository.FindCountAsync();

//         return Result<long>.Ok(count);
//     }
//     catch (Exception ex)
//     {
//         _logger.LogError(ex, "Unexpected error while counting departments.");
//         return Result<long>.Fail("An unexpected error occurred. Please try again later.", ErrorType.UnexpectedError);
//     }
// }

// ========== Return DTO Implmentation =====================================
// public async Task<DepartmentGetDto> UpdateAsync(string departmentCode, DepartmentUpdateDto updateDto)
// {
//     try
//     {
//         if (string.IsNullOrEmpty(departmentCode) || string.IsNullOrWhiteSpace(departmentCode))
//             throw new ArgumentNullException(nameof(departmentCode), "Department code is required.");

//         // Validate existence
//         var targetDepartment = await _unitOfWork.DepartmentRepository.FindByDepartmentCodeAsync(departmentCode);
//         if (targetDepartment == null)
//             // TODO: NotFoundException
//             throw new NotFoundException("Department not found.");

//         // Handle concurrency (example using row version)
//         // if (existing.RowVersion != department.RowVersion)
//         //     return Result<Department>.Fail("Concurrency conflict.");

//         // Track changes

//         _unitOfWork.DepartmentRepository.Update(targetDepartment);
//         targetDepartment.DepartmentName = updateDto.DepartmentName;

//         await _unitOfWork.SaveChangesAsync();

//         return targetDepartment.ToGetDto();
//     }
//     catch (DbUpdateException ex)
//     {
//         if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
//         {
//             _logger.LogError(ex, pgEx.MessageText);
//             // TODO: DuplicateEntityException
//             throw new DuplicateContentException("Duplicate entry exception occurred.");
//         }
//         else
//         {
//             // Handle database-specific errors
//             _logger.LogError(ex, "Database error while updating department.");
//             // TODO: DatabaseException
//             throw new Exception("A database error occurred.");
//         }
//         // Handle database-specific errors
//     }
//     catch (Exception ex)
//     {
//         // Log unexpected errors
//         _logger.LogError(ex, "Unexpected error while updating department.");
//         throw new Exception("An unexpected error occurred. Please try again later.");

//     }
// }

// public async Task<bool> DeleteAsync(string departmentCode)
// {
//     try
//     {
//         var department = await _unitOfWork.DepartmentRepository.FindByDepartmentCodeAsync(departmentCode);
//         if (department == null)
//             return true; // Idempotent: Treat as success

//         if (department == null)
//             // TODO: NotFoundException
//             throw new NotFoundException("Department not found.");

//         _unitOfWork.DepartmentRepository.Delete(department);
//         await _unitOfWork.SaveChangesAsync();

//         return true;
//     }
//     catch (DbUpdateException ex)
//     {
//         // Handle database-specific errors
//         _logger.LogError(ex, "Database error while deleting department.");
//         // TODO: DatabaseException
//         throw new Exception("A database error occurred.");
//     }
//     catch (Exception ex)
//     {
//         // Log unexpected errors
//         _logger.LogError(ex, "Unexpected error while deleting department.");
//         // throw; // Propagate to global exception handler
//         throw new Exception("An unexpected error occurred. Please try again later.");
//     }
// }

// public async Task<IEnumerable<DepartmentGetDto>> FindAllAsync()
// {
//     try
//     {
//         var departments = await _unitOfWork.DepartmentRepository.FindAllAsync();

//         return departments.ToGetDto();
//     }
//     catch (Exception ex)
//     {
//         // Log unexpected errors
//         _logger.LogError(ex, "Unexpected error while querying departments.");
//         // throw; // Propagate to global exception handler
//         throw new Exception("An unexpected error occurred. Please try again later.");
//     }
// }

// Task<IEnumerable<DepartmentGetDto>> FindAll_Paginated_Async()
// {
//     try
//     {
//         var departments = await _unitOfWork.DepartmentRepository.FindAllAsync();

//         return departments.ToGetDto();
//     }
//     catch (Exception ex)
//     {
//         // Log unexpected errors
//         _logger.LogError(ex, "Unexpected error while querying departments.");
//         // throw; // Propagate to global exception handler
//         throw new Exception("An unexpected error occurred. Please try again later.");
//     }
// }

// public async Task<IEnumerable<DepartmentDto>> FindAllInsecureAsync()
// {
//     try
//     {
//         var departments = await _unitOfWork.DepartmentRepository.FindAllAsync();

//         return departments.Select(e => new DepartmentDto
//         {
//             Id = e.Id,
//             DepartmentCode = e.DepartmentCode,
//             DepartmentName = e.DepartmentName
//         }).ToList();
//     }
//     catch (Exception ex)
//     {
//         // Log unexpected errors
//         _logger.LogError(ex, "Unexpected error while querying departments.");
//         // throw; // Propagate to global exception handler
//         throw new Exception("An unexpected error occurred. Please try again later.");
//     }
// }

// public async Task<IEnumerable<DepartmentDto>> FindAllInsecureAsync(int pageNumber = 1, int pageSize = 20)
// {
//     try
//     {
//         var departments = await _unitOfWork.DepartmentRepository.FindAllAsQueryable()
//             .Skip((pageNumber - 1) * pageSize)
//             .Take(pageSize)
//             .ToListAsync();

//         return departments.Select(e => new DepartmentDto
//         {
//             Id = e.Id,
//             DepartmentCode = e.DepartmentCode,
//             DepartmentName = e.DepartmentName
//         }).ToList();
//     }
//     catch (Exception ex)
//     {
//         // Log unexpected errors
//         _logger.LogError(ex, "Unexpected error while querying departments.");
//         // throw; // Propagate to global exception handler
//         throw new Exception("An unexpected error occurred. Please try again later.");
//     }
// }

// public async Task<int> FindCountAsync()
// {
//     try
//     {
//         var count = await _unitOfWork.DepartmentRepository.FindCountAsync();

//         return count;
//     }
//     catch (Exception ex)
//     {
//         _logger.LogError(ex, "Unexpected error while counting departments.");
//         throw new Exception("An unexpected error occurred. Please try again later.");
//     }

// }