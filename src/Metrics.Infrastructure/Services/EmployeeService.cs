using Metrics.Application.Domains;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Application.Interfaces.IServices;
using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Metrics.Infrastructure.Services;

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

    public async Task<Employee> CreateAsync(Employee employee)
    {
        // if (await _employeeRepository.EmployeeExistsAsync(employee.EmployeeCode.ToString()))
        //     throw new DuplicateContentException("Employee code already exist.");

        try
        {
            _employeeRepository.Create(employee);
            await _context.SaveChangesAsync();

            return employee;
        }
        // catch (DbUpdateException ex)
        // {
        //     // when (ex.InnerException is PostgresException postgresEx && postgresEx.SqlState == "23505")
        //     if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
        //     {
        //         _logger.LogError(ex, pgEx.MessageText);
        //         // TODO: DuplicateEntityException
        //         throw new DuplicateContentException("Employee already exist.", ex.InnerException);
        //     }
        //     else
        //     {
        //         // Handle database-specific errors
        //         _logger.LogError(ex, "Database error while creating department.");
        //         // TODO: DatabaseException
        //         throw new Exception("A database error occurred.");
        //     }
        // }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating department.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<Employee> UpdateAsync(string employeeCode, Employee employee)
    {
        if (string.IsNullOrEmpty(employeeCode) || string.IsNullOrWhiteSpace(employeeCode))
            throw new ArgumentNullException("Employee code is required.");

        // TODO: does this check necessary?
        if (employeeCode != employee.EmployeeCode)
            throw new DbUpdateException("Update failed. Hint: employeeCode to update and existing employeeCode does not match.");

        try
        {
            var targetEmployee = await _employeeRepository.FindByEmployeeCodeAsync(employeeCode);
            if (targetEmployee == null)
                throw new NotFoundException($"Employee with code {employeeCode} not found.");

            // Handle concurrency (example using row version)
            // if (existing.RowVersion != department.RowVersion)
            //     return Result<Department>.Fail("Concurrency conflict.");

            // Note: This is full update (**not partial update)
            _employeeRepository.Update(targetEmployee);
            targetEmployee.EmployeeCode = employee.EmployeeCode;
            targetEmployee.FullName = employee.FullName;
            targetEmployee.PhoneNumber = employee.PhoneNumber;
            targetEmployee.Address = employee.Address;
            targetEmployee.DepartmentId = employee.DepartmentId;
            targetEmployee.ApplicationUserId = employee.ApplicationUserId;
            await _context.SaveChangesAsync();

            // refetch updated entity
            var updatedEntity = await _employeeRepository.FindByEmployeeCodeAsync(employeeCode);
            return updatedEntity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating employee.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<bool> DeleteAsync(string employeeCode)
    {
        if (string.IsNullOrEmpty(employeeCode) || string.IsNullOrWhiteSpace(employeeCode))
            throw new ArgumentNullException("Employee code is required.");

        try
        {
            var employee = await _employeeRepository.FindByEmployeeCodeAsync(employeeCode);

            if (employee == null)
                throw new NotFoundException($"Employee with code {employeeCode} not found.");
            // or return true; // Idempotent: Treat as success

            _employeeRepository.Delete(employee);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting employee.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<Employee> FindByIdAsync(long id)
    {
        try
        {
            if (id <= 0)
                throw new Exception("Parameter id is required.");

            var employee = await _employeeRepository.FindByIdAsync(id);
            if (employee == null)
                throw new Exception($"Employee with id {id} not found.");

            return employee;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying employee by employee id.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<Employee?> FindByEmployeeCodeAsync(string employeeCode)
    {
        try
        {
            if (string.IsNullOrEmpty(employeeCode))
                throw new Exception("Parameter employeeCode is required.");

            var employee = await _employeeRepository.FindByEmployeeCodeAsync(employeeCode);
            if (employee == null)
                throw new Exception($"Employee with code {employeeCode} not found.");

            return employee;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying department by department code.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<IEnumerable<Employee>> FindAllAsync()
    {
        try
        {
            var employees = await _employeeRepository.FindAllAsQueryable()
                .Include(e => e.Department)
                .Include(e => e.ApplicationUser)
                .Include(e => e.KpiSubmissions)
                .ToListAsync();

            return employees;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying employees.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<long> FindByUserIdAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrWhiteSpace(userId))
            throw new Exception("Parameter userId is required.");

        try
        {
            var employeeId = await _employeeRepository.FindAllAsQueryable()
                .Where(e => e.ApplicationUserId == userId)
                .Select(e => e.Id)
                .FirstOrDefaultAsync();

            if (employeeId <= 0)
                throw new NotFoundException("Employee not found.");

            return employeeId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying employee id by user id.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }


    // ========== Return DTO ===================================================
    // public async Task<EmployeeGetDto> CreateAsync(EmployeeCreateDto createDto)
    // {
    //     try
    //     {
    //         if (await _employeeRepository.EmployeeExistsAsync(createDto.EmployeeCode.ToString()))
    //             // TODO: DuplicateEntityException
    //             throw new DuplicateContentException("Employee code already exist.");

    //         var entity = createDto.ToEntity();
    //         _employeeRepository.Create(entity);
    //         await _context.SaveChangesAsync();

    //         return createDto.ToGetDto();
    //     }
    //     catch (DbUpdateException ex)
    //     {
    //         // when (ex.InnerException is PostgresException postgresEx && postgresEx.SqlState == "23505")
    //         if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
    //         {
    //             _logger.LogError(ex, pgEx.MessageText);
    //             // TODO: DuplicateEntityException
    //             throw new DuplicateContentException("Employee already exist.", ex.InnerException);
    //         }
    //         else
    //         {
    //             // Handle database-specific errors
    //             _logger.LogError(ex, "Database error while creating department.");
    //             // TODO: DatabaseException
    //             throw new Exception("A database error occurred.");
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         // Log unexpected errors
    //         _logger.LogError(ex, "Unexpected error while creating department.");
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }

    // public async Task<EmployeeGetDto> UpdateAsync(string employeeCode, EmployeeUpdateDto updateDto)
    // {
    //     try
    //     {
    //         // Validate existence
    //         var targetEmployee = await _employeeRepository.FindByEmployeeCodeAsync(employeeCode);
    //         if (targetEmployee == null)
    //             // TODO: NotFoundException
    //             throw new NotFoundException("Employee not found.");

    //         // Handle concurrency (example using row version)
    //         // if (existing.RowVersion != department.RowVersion)
    //         //     return Result<Department>.Fail("Concurrency conflict.");

    //         // Track changes

    //         _employeeRepository.Update(targetEmployee);
    //         targetEmployee.FullName = updateDto.FullName;
    //         targetEmployee.PhoneNumber = updateDto.PhoneNumber;
    //         targetEmployee.Address = updateDto.Address;
    //         targetEmployee.DepartmentId = updateDto.DepartmentId;

    //         await _context.SaveChangesAsync();

    //         return targetEmployee.ToGetDto();
    //     }
    //     catch (DbUpdateException ex)
    //     {
    //         if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
    //         {
    //             _logger.LogError(ex, pgEx.MessageText);
    //             // TODO: DuplicateEntityException
    //             throw new Exception("Duplicate entry exception occurred.");
    //         }
    //         else
    //         {
    //             // Handle database-specific errors
    //             _logger.LogError(ex, "Database error while updating employee.");
    //             // TODO: DatabaseException
    //             throw new Exception("A database error occurred.");
    //         }
    //         // Handle database-specific errors
    //     }
    //     catch (Exception ex)
    //     {
    //         // Log unexpected errors
    //         _logger.LogError(ex, "Unexpected error while updating employee.");
    //         throw new Exception("An unexpected error occurred. Please try again later.");

    //     }
    // }

    // public async Task<bool> DeleteAsync(string employeeCode)
    // {
    //     try
    //     {
    //         var employee = await _employeeRepository.FindByEmployeeCodeAsync(employeeCode);
    //         if (employee == null)
    //             return true; // Idempotent: Treat as success

    //         if (employee == null)
    //             // TODO: NotFoundException
    //             throw new Exception("Employee not found.");

    //         _employeeRepository.Delete(employee);
    //         await _context.SaveChangesAsync();

    //         return true;
    //     }
    //     catch (DbUpdateException ex)
    //     {
    //         // Handle database-specific errors
    //         _logger.LogError(ex, "Database error while deleting employee.");
    //         // TODO: DatabaseException
    //         throw new Exception("A database error occurred.");
    //     }
    //     catch (Exception ex)
    //     {
    //         // Log unexpected errors
    //         _logger.LogError(ex, "Unexpected error while deleting employee.");
    //         // throw; // Propagate to global exception handler
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }

    // public async Task<EmployeeGetDto> FindByIdAsync(long id)
    // {
    //     try
    //     {
    //         if (id <= 0)
    //             // TODO: ValidationException
    //             throw new Exception("Parameter id is required.");

    //         var employee = await _employeeRepository.FindByIdAsync(id);
    //         if (employee == null)
    //             // TODO: NotFoundException
    //             throw new Exception($"Employee with id {id} not found.");

    //         return employee.ToGetDto();
    //     }
    //     catch (Exception ex)
    //     {
    //         // Log unexpected errors
    //         _logger.LogError(ex, "Unexpected error while querying employee by employee id.");
    //         // throw; // Propagate to global exception handler
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }

    // public async Task<EmployeeGetDto> FindByEmployeeCodeAsync(string employeeCode)
    // {
    //     try
    //     {
    //         if (string.IsNullOrEmpty(employeeCode))
    //             // TODO: ValidationException
    //             throw new Exception("Parameter employeeCode is required.");

    //         var employee = await _employeeRepository.FindByEmployeeCodeAsync(employeeCode);
    //         if (employee == null)
    //             // TODO: NotFoundException
    //             throw new Exception($"Employee with code {employeeCode} not found.");

    //         return employee.ToGetDto();
    //     }
    //     catch (Exception ex)
    //     {
    //         // Log unexpected errors
    //         _logger.LogError(ex, "Unexpected error while querying department by department code.");
    //         // throw; // Propagate to global exception handler
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }


    // // public async Task<IEnumerable<EmployeeGetDto>> FindAllAsync()
    // // {
    // //     try
    // //     {
    // //         var employees = await _employeeRepository.FindAllAsync();

    // //         return employees.ToGetDto();
    // //     }
    // //     catch (Exception ex)
    // //     {
    // //         // Log unexpected errors
    // //         _logger.LogError(ex, "Unexpected error while querying employees.");
    // //         // throw; // Propagate to global exception handler
    // //         throw new Exception("An unexpected error occurred. Please try again later.");
    // //     }
    // // }

    // public async Task<IEnumerable<EmployeeGetAllDto>> FindAllAsync()
    // {
    //     try
    //     {
    //         var employees = await _employeeRepository.FindAllAsQueryable()
    //             .Include(e => e.CurrentDepartment)
    //             // .Include(e => e.UserAccount)
    //             .Include(e => e.KpiSubmissions)
    //             .ToListAsync();

    //         return employees.Select(e => new EmployeeGetAllDto
    //         {
    //             EmployeeCode = e.EmployeeCode,
    //             FullName = e.FullName,
    //             Address = e.Address,
    //             PhoneNumber = e.PhoneNumber,
    //             DepartmentId = e.DepartmentId,
    //             CurrentDepartment = e.CurrentDepartment,
    //             ApplicationUserId = e.ApplicationUserId
    //         }).ToList();
    //     }
    //     catch (Exception ex)
    //     {
    //         // Log unexpected errors
    //         _logger.LogError(ex, "Unexpected error while querying employees.");
    //         // throw; // Propagate to global exception handler
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }

    // public async Task<long> FindByUserIdAsync(string userId)
    // {
    //     try
    //     {
    //         if (string.IsNullOrEmpty(userId) || string.IsNullOrWhiteSpace(userId))
    //             throw new Exception("Parameter userId is required.");

    //         var employeeId = await _employeeRepository.FindAllAsQueryable()
    //             .Where(e => e.ApplicationUserId == userId)
    //             .Select(e => e.Id)
    //             .FirstOrDefaultAsync();

    //         if (employeeId <= 0)
    //             throw new NotFoundException("Employee not found.");

    //         return employeeId;
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Unexpected error while querying employee id by user id.");
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }
}
