using Metrics.Application.DTOs.DepartmentDtos;
using Metrics.Application.Mappers.DtoMappers;
using Metrics.Application.Results;
using Metrics.Application.Services.IServices;
using Metrics.Domain.Entities;
using Metrics.Domain.Exceptions;
using Metrics.Infrastructure;
using Metrics.Infrastructure.Data;
using Metrics.Infrastructure.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Metrics.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly MetricsDbContext _context;
    private readonly ILogger<DepartmentService> _logger;
    private readonly IDepartmentRepository _departmentRepository;

    public DepartmentService(
        MetricsDbContext context,
        ILogger<DepartmentService> logger,
        IDepartmentRepository departmentRepository)
    {
        _context = context;
        _logger = logger;
        _departmentRepository = departmentRepository;
    }

    public async Task<Result<Department>> CreateAsync(Department entity)
    {
        try
        {
            if (await _departmentRepository.DepartmentExistsAsync(entity.DepartmentCode.ToString()))
                return Result<Department>.Fail("Department already exist.", ErrorType.DuplicateKey);

            _departmentRepository.Create(entity);
            await _context.SaveChangesAsync();

            return Result<Department>.Ok(entity);
        }
        catch (DbUpdateException ex)
        {
            // when (ex.InnerException is PostgresException postgresEx && postgresEx.SqlState == "23505")
            if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                _logger.LogError(ex, pgEx.MessageText);
                return Result<Department>.Fail("A database error occurred.", ErrorType.DuplicateKey);
            }
            else
            {
                // Handle database-specific errors
                _logger.LogError(ex, "Database error while creating department.");
                return Result<Department>.Fail("A database error occurred.", ErrorType.DatabaseError);
            }
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while creating department.");
            return Result<Department>.Fail("An unexpected error occurred. Please try again later.", ErrorType.UnexpectedError);
        }
    }

    public async Task<Result<bool>> DeleteAsync(string departmentCode)
    {
        try
        {
            var department = await _departmentRepository.FindByDepartmentCodeAsync(departmentCode);
            if (department == null)
                return Result<bool>.Ok(true); // Idempotent: Treat as success
            // if (department == null)
            //     return Result<Department>.Fail("Department not exist.");
            _departmentRepository.Delete(department);
            await _context.SaveChangesAsync();

            return Result<bool>.Ok(true);
        }
        catch (DbUpdateException ex)
        {
            // Handle database-specific errors
            _logger.LogError(ex, "Database error while deleting department.");
            return Result<bool>.Fail("A database error occurred.", ErrorType.DatabaseError);
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while deleting department.");
            // throw; // Propagate to global exception handler
            return Result<bool>.Fail("An unexpected error occurred. Please try again later.", ErrorType.UnexpectedError);
        }
    }

    public async Task<Result<IEnumerable<Department>>> FindAllAsync()
    {
        try
        {
            var departments = await _departmentRepository.FindAllAsync();

            return Result<IEnumerable<Department>>.Ok(departments);
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while querying departments.");
            // throw; // Propagate to global exception handler
            return Result<IEnumerable<Department>>.Fail("An unexpected error occurred. Please try again later.", ErrorType.UnexpectedError);
        }
    }

    public async Task<Result<Department>> FindByDepartmentCodeAsync(string departmentCode)
    {
        try
        {
            var department = await _departmentRepository.FindByDepartmentCodeAsync(departmentCode);
            if (department == null)
                // throw new Exception($"Department with code {departmentCode} not found.");
                return Result<Department>.Fail("Department not found.", ErrorType.NotFound);

            return Result<Department>.Ok(department);
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while querying department by department code.");
            // throw; // Propagate to global exception handler
            return Result<Department>.Fail("An unexpected error occurred. Please try again later.", ErrorType.UnexpectedError);
        }

    }

    public async Task<Result<Department>> FindByIdAsync(long id)
    {
        try
        {
            var department = await _departmentRepository.FindByIdAsync(id);
            if (department == null)
                // throw new Exception($"Department with code {departmentCode} not found.");
                return Result<Department>.Fail("Department not found.", ErrorType.NotFound);

            return Result<Department>.Ok(department);
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while querying department by id.");
            // throw; // Propagate to global exception handler
            return Result<Department>.Fail("An unexpected error occurred. Please try again later.", ErrorType.UnexpectedError);
        }


    }

    public async Task<Result<Department>> UpdateAsync(Department entity)
    {
        try
        {
            // Validate existence
            var targetDepartment = await _departmentRepository.FindByDepartmentCodeAsync(entity.DepartmentCode.ToString());
            if (targetDepartment == null)
                return Result<Department>.Fail("Department not found.", ErrorType.NotFound);

            // Handle concurrency (example using row version)
            // if (existing.RowVersion != department.RowVersion)
            //     return Result<Department>.Fail("Concurrency conflict.");

            // Track changes
            _departmentRepository.Update(targetDepartment);
            targetDepartment.DepartmentName = entity.DepartmentName;

            await _context.SaveChangesAsync();

            return Result<Department>.Ok(entity);
        }
        catch (DbUpdateException ex)
        {
            // Handle database-specific errors
            _logger.LogError(ex, "Database error while updating department.");
            return Result<Department>.Fail("A database error occurred.", ErrorType.DatabaseError);
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while updating departments.");
            return Result<Department>.Fail("An unexpected error occurred. Please try again later.", ErrorType.UnexpectedError);
        }
    }


    public async Task<Result<Department>> Update2Async(string departmentCode, Department entity)
    {
        try
        {
            // Validate existence
            var targetDepartment = await _departmentRepository.FindByDepartmentCodeAsync(entity.DepartmentCode.ToString());
            if (targetDepartment == null)
                return Result<Department>.Fail("Department not found.", ErrorType.NotFound);

            // Handle concurrency (example using row version)
            // if (existing.RowVersion != department.RowVersion)
            //     return Result<Department>.Fail("Concurrency conflict.");

            // Track changes
            _departmentRepository.Update(targetDepartment);
            targetDepartment.DepartmentName = entity.DepartmentName;

            await _context.SaveChangesAsync();

            return Result<Department>.Ok(entity);
        }
        catch (DbUpdateException ex)
        {
            // Handle database-specific errors
            _logger.LogError(ex, "Database error while updating department.");
            return Result<Department>.Fail("A database error occurred.", ErrorType.DatabaseError);
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while updating departments.");
            return Result<Department>.Fail("An unexpected error occurred. Please try again later.", ErrorType.UnexpectedError);
        }
    }

    public async Task<DepartmentGetDto> FindByDepartmentCode_Async(string departmentCode)
    {
        try
        {
            if (string.IsNullOrEmpty(departmentCode))
                // TODO: ValidationException
                throw new Exception("Parameter departmentCode is required.");

            var department = await _departmentRepository.FindByDepartmentCodeAsync(departmentCode);
            if (department == null)
                // TODO: NotFoundException
                throw new Exception($"Department with code {departmentCode} not found.");

            return department.ToGetDto();
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while querying department by department code.");
            // throw; // Propagate to global exception handler
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }


    public async Task<DepartmentGetDto> FindById_Async(long id)
    {
        try
        {
            if (id <= 0)
                // TODO: ValidationException
                throw new Exception("Parameter id is required.");

            var department = await _departmentRepository.FindByIdAsync(id);
            if (department == null)
                // TODO: NotFoundException
                throw new NotFoundException($"Department with id {id} not found.");

            return department.ToGetDto();
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while querying department by department code.");
            // throw; // Propagate to global exception handler
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<DepartmentGetDto> Create_Async(DepartmentCreateDto createDto)
    {
        try
        {
            if (await _departmentRepository.DepartmentExistsAsync(createDto.DepartmentName.ToString()))
                // TODO: DuplicateEntityException
                throw new DuplicateContentException("Department name already exist.");

            var entity = createDto.ToEntity();
            _departmentRepository.Create(entity);
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

    public async Task<DepartmentGetDto> Update_Async(string departmentCode, DepartmentUpdateDto updateDto)
    {
        try
        {
            if (string.IsNullOrEmpty(departmentCode) || string.IsNullOrWhiteSpace(departmentCode))
                throw new ArgumentNullException(nameof(departmentCode), "Department code is required.");

            // Validate existence
            var targetDepartment = await _departmentRepository.FindByDepartmentCodeAsync(departmentCode);
            if (targetDepartment == null)
                // TODO: NotFoundException
                throw new NotFoundException("Department not found.");

            // Handle concurrency (example using row version)
            // if (existing.RowVersion != department.RowVersion)
            //     return Result<Department>.Fail("Concurrency conflict.");

            // Track changes

            _departmentRepository.Update(targetDepartment);
            targetDepartment.DepartmentName = updateDto.DepartmentName;

            await _context.SaveChangesAsync();

            return targetDepartment.ToGetDto();
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                _logger.LogError(ex, pgEx.MessageText);
                // TODO: DuplicateEntityException
                throw new DuplicateContentException("Duplicate entry exception occurred.");
            }
            else
            {
                // Handle database-specific errors
                _logger.LogError(ex, "Database error while updating department.");
                // TODO: DatabaseException
                throw new Exception("A database error occurred.");
            }
            // Handle database-specific errors
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while updating department.");
            throw new Exception("An unexpected error occurred. Please try again later.");

        }
    }

    public async Task<bool> Delete_Async(string departmentCode)
    {
        try
        {
            var department = await _departmentRepository.FindByDepartmentCodeAsync(departmentCode);
            if (department == null)
                return true; // Idempotent: Treat as success

            if (department == null)
                // TODO: NotFoundException
                throw new NotFoundException("Department not found.");

            _departmentRepository.Delete(department);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (DbUpdateException ex)
        {
            // Handle database-specific errors
            _logger.LogError(ex, "Database error while deleting department.");
            // TODO: DatabaseException
            throw new Exception("A database error occurred.");
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while deleting department.");
            // throw; // Propagate to global exception handler
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<IEnumerable<DepartmentGetDto>> FindAll_Async()
    {
        try
        {
            var departments = await _departmentRepository.FindAllAsync();

            return departments.ToGetDto();
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while querying departments.");
            // throw; // Propagate to global exception handler
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<IEnumerable<DepartmentDto>> FindAllInsecure_Async()
    {
        try
        {
            var departments = await _departmentRepository.FindAllAsync();

            return departments.Select(e => new DepartmentDto
            {
                Id = e.Id,
                DepartmentCode = e.DepartmentCode,
                DepartmentName = e.DepartmentName
            }).ToList();
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while querying departments.");
            // throw; // Propagate to global exception handler
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<IEnumerable<DepartmentDto>> FindAllInsecure_Async(int pageNumber = 1, int pageSize = 20)
    {
        try
        {
            var departments = await _departmentRepository.FindAllAsQueryable()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return departments.Select(e => new DepartmentDto
            {
                Id = e.Id,
                DepartmentCode = e.DepartmentCode,
                DepartmentName = e.DepartmentName
            }).ToList();
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while querying departments.");
            // throw; // Propagate to global exception handler
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<int> FindCount_Async()
    {
        try
        {
            var count = await _departmentRepository.FindCountAsync();

            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while counting departments.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }

    }



    // private readonly IUnitOfWork _unitOfWork;
    // private readonly ILogger<DepartmentService> _logger;

    // public DepartmentService(IUnitOfWork unitOfWork, ILogger<DepartmentService> logger)
    // {
    //     _unitOfWork = unitOfWork;
    //     _logger = logger;
    // }

    // // ========== Option 1: Return DTO =========================================
    // public async Task<DepartmentGetDto?> GetByIdAsync(long id)
    // {
    //     var department = await _unitOfWork.DepartmentRepository.FindByIdAsync(id);

    //     if (department == null)
    //         throw new Exception($"Department with id {id} does not exist.");

    //     return department.ToGetDto()
    //         ?? throw new Exception("Mapping to DTO failed.");
    // }

    // public async Task<DepartmentGetDto?> GetByDepartmentCodeAsync(string departmentCode)
    // {
    //     var department = await _unitOfWork.DepartmentRepository.FindByDepartmentCodeAsync(departmentCode);

    //     if (department == null)
    //         throw new Exception($"Department with code {departmentCode} does not exist.");

    //     return department.ToGetDto()
    //         ?? throw new Exception("Mapping to DTO failed.");
    // }

    // public async Task<IEnumerable<DepartmentGetDto>> GetAllAsync()
    // {
    //     var department = await _unitOfWork.DepartmentRepository.FindAllAsync();
    //     if (department == null)
    //         return [];

    //     return DepartmentDtoMapper.ToGetDto(department);
    // }

    // [Obsolete("Experimental!", false)]
    // public async Task<IEnumerable<DepartmentGetDto>> SearchAsync(string keyword)
    // {
    //     var query = _unitOfWork.DepartmentRepository.FindAll()
    //         .Where(e => e.DepartmentName.Contains(keyword));
    //     var departmentList = await query.ToListAsync();
    //     if (departmentList == null)
    //         return [];

    //     return departmentList.ToGetDto();
    // }

    // public async Task<DepartmentGetDto> CreateAsync(DepartmentCreateDto createDto)
    // {
    //     // if (await _unitOfWork.DepartmentRepository.DepartmentExistAsync(createDto.DepartmentCode.ToString()))
    //     //     //DuplicateKeyException
    //     //     throw new InvalidOperationException($"Department code '{createDto.DepartmentCode}' already exists.");
    //     try
    //     {
    //         var entity = createDto.ToEntity();
    //         await _unitOfWork.DepartmentRepository.CreateAsync(entity);
    //         var saved = await _unitOfWork.SaveChangesAsync();
    //         if (saved > 0)
    //         {
    //             //var createdDepartment = await FindByIdAsync(entity.Id);
    //             //return createdDepartment
    //             return createDto.ToGetDto();
    //         }

    //         throw new Exception("Create new department failed.");
    //     }
    //     catch (DbUpdateException ex) when (ex.InnerException is PostgresException postgresEx && postgresEx.SqlState == "23505")
    //     {
    //         _logger.LogWarning(ex.Message);
    //         throw new InvalidOperationException("Duplicate content.");
    //     }
    //     catch (System.Exception)
    //     {
    //         throw new Exception("Create new department failed.");
    //     }
    // }

    // public Task<DepartmentGetDto> UpdateAsync(DepartmentUpdateDto updateDto)
    // {
    //     throw new NotImplementedException();
    //     //var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(dto.Id);
    //     //if (department == null)
    //     //    throw new NotFoundException($"Department with ID {dto.Id} not found.");

    //     //department.Name = dto.Name;
    //     //department.Description = dto.Description;

    //     //_unitOfWork.DepartmentRepository.Update(department);
    //     //await _unitOfWork.SaveChangesAsync();

    //     //return UserMapper.ToDto(department);
    // }

    // public async Task<bool> DeleteAsync(string departmentCode)
    // {
    //     var department = await _unitOfWork.DepartmentRepository.FindByDepartmentCodeAsync(departmentCode);
    //     if (department == null)
    //         //NotFoundException
    //         throw new Exception($"Department with code {departmentCode} not found.");

    //     _unitOfWork.DepartmentRepository.Delete(department);

    //     return await _unitOfWork.SaveChangesAsync() > 0;
    // }


    // // ========== Option 2: Return Entity ======================================
    // public async Task<Department?> GetBy_DepartmentCode_Async(string departmentCode)
    // {
    //     var department = await _unitOfWork.DepartmentRepository.FindByDepartmentCodeAsync(departmentCode);

    //     if (department == null)
    //         throw new Exception($"Department with code {departmentCode} does not exist.");

    //     return department;
    // }

    // public async Task<IEnumerable<Department>> Search_Async(string keyword)
    // {
    //     var query = await _unitOfWork.DepartmentRepository
    //         .FindAny(e => e.DepartmentName.Contains(keyword))
    //         .ToListAsync();

    //     if (query.Count == 0)
    //         return [];

    //     return query;
    // }

    // public async Task<Department?> GetBy_IdAsync(long id)
    // {
    //     var department = await _unitOfWork.DepartmentRepository.FindByIdAsync(id);

    //     if (department == null)
    //         throw new Exception($"Department with id {id} does not exist.");

    //     return department;
    // }

    // public async Task<Department> Create_Async(Department entity)
    // {
    //     try
    //     {
    //         var newDepartment = await _unitOfWork.DepartmentRepository.CreateAsync(entity);
    //         var saved = await _unitOfWork.SaveChangesAsync();
    //         if (saved > 0)
    //         {
    //             return await GetBy_IdAsync(newDepartment.Id)
    //                 ?? throw new Exception("Fetching created department failed.");
    //         }

    //         throw new Exception("Creating new department failed.");
    //     }
    //     catch (DbUpdateException dbEx)
    //         when (dbEx.InnerException is PostgresException postgresEx
    //             && postgresEx.SqlState == "23505")
    //     {
    //         _logger.LogWarning(dbEx.Message);
    //         throw new InvalidOperationException("Duplicate content.");
    //     }
    //     catch (Exception ex)
    //     {
    //         throw new Exception("Create new department failed.", ex);
    //     }
    // }

    // public async Task<Department> Update_Async(string departmentCode, Department entity)
    // {
    //     var targetDepartment = await _unitOfWork.DepartmentRepository.FindByDepartmentCodeAsync(departmentCode);
    //     try
    //     {
    //         _unitOfWork.DepartmentRepository.Update(entity);
    //         var updated = await _unitOfWork.SaveChangesAsync();
    //         if (updated > 0)
    //         {
    //             return await GetBy_IdAsync(entity.Id)
    //                 ?? throw new Exception("Fetching updated department failed.");
    //         }

    //         throw new Exception("Creating new department failed.");
    //     }
    //     catch (Exception ex)
    //     {
    //         throw new Exception("Update new department failed.", ex);
    //     }
    // }

    // public async Task<bool> Delete_Async(string departmentCode)
    // {
    //     try
    //     {
    //         var department = await _unitOfWork.DepartmentRepository.FindByDepartmentCodeAsync(departmentCode);
    //         if (department == null)
    //             throw new Exception($"Department with code {departmentCode} not found.");

    //         _unitOfWork.DepartmentRepository.Delete(department);

    //         return await _unitOfWork.SaveChangesAsync() > 0;
    //     }
    //     catch (Exception ex)
    //     {
    //         throw new Exception("Delete department failed.", ex);
    //     }
    // }

    // public async Task<IEnumerable<Department>> GetAll_Async()
    // {
    //     var departments = await _unitOfWork.DepartmentRepository.FindAllAsync();
    //     if (departments == null)
    //         return [];

    //     return departments;
    // }

}


// ===========================================================================
// public async Task<Department> FindByIdAsync(long id)
// {
//     var result = await _unitOfWork.DepartmentRepository.FindByIdAsync(id);
//     if (result != null)
//     {
//         return result;
//     }

//     _logger.LogWarning("FindByIdAsync: id not found");
//     throw new Exception("Department not found.");
// }

// public async Task<Department?> FindByDepartmentCodeAsync(string departmentCode)
// {
//     var result = await _unitOfWork.DepartmentRepository.FindByDepartmentCodeAsync(departmentCode);
//     if (result != null)
//     {
//         return result;
//     }

//     throw new Exception($"Department of code {departmentCode} not found.");
// }


// public async Task<Department> CreateAsync(Department entity)
// {
//     var entityEntry = await _unitOfWork.DepartmentRepository.CreateAsync2(entity);
//     var saved = await _unitOfWork.SaveChangesAsync();
//     if (saved > 0)
//     {
//         var createdDepartment = await FindByIdAsync(entityEntry.Entity.Id);
//         return createdDepartment;
//     }

//     throw new Exception("Create new department failed.");
// }

// public async Task<Department> UpdateAsync(long id, Department entity)
// {
//     var targetEntity = await FindByIdAsync(id);

//     if (targetEntity != null)
//     {
//         _unitOfWork.DepartmentRepository.Update(entity);
//         var updated = await _unitOfWork.SaveChangesAsync();
//         if (updated > 0)
//         {
//             return await FindByIdAsync(id);
//         }

//         throw new Exception("Update department failed.");
//     }

//     throw new Exception("Department to update not exist.");
// }

// public async Task<Department> UpdateByDepartmentCodeAsync(string departmentCode, Department entity)
// {
//     var targetEntity = await FindByDepartmentCodeAsync(departmentCode);
//     try
//     {
//         if (targetEntity != null)
//         {
//             targetEntity.DepartmentCode = entity.DepartmentCode;
//             targetEntity.DepartmentName = entity.DepartmentName;
//             _unitOfWork.DepartmentRepository.Update(targetEntity);
//             var updated = await _unitOfWork.SaveChangesAsync();

//             if (updated > 0)
//             {
//                 return await FindByDepartmentCodeAsync(entity.DepartmentCode.ToString())
//                     ?? throw new Exception("Retriving updated department failed.");
//             }

//             throw new Exception("Update department failed.");
//         }
//         throw new Exception("Department to update not exist.");
//     }
//     catch (DbUpdateException exDbUpdate)
//     {
//         //if (exDbUpdate.InnerException is NpgsqlException npgsqlEx)
//         //{

//         //}
//         throw new Exception("Department update error. (Database Update Exception)", exDbUpdate);

//     }
//     catch (Exception ex)
//     {
//         throw new Exception("Department to update not exist.", ex);
//     }
// }



// public async Task<bool> Delete(string departmentCode)
// {
//     var entity = await FindByDepartmentCodeAsync(departmentCode);
//     if (entity != null)
//     {
//         _unitOfWork.DepartmentRepository.Delete(entity);

//         return await _unitOfWork.SaveChangesAsync() > 0;
//     }

//     return false;
// }

// public async Task<IEnumerable<Department>> FindAllAsync()
// {
//     // do some works, query
//     // ...
//     return await _unitOfWork.DepartmentRepository.FindAllAsync();

// }

