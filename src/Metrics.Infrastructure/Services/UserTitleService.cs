using Metrics.Application.Common.Mappers;
using Metrics.Application.Domains;
using Metrics.Application.DTOs.UserGroup;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Application.Interfaces.IServices;
using Metrics.Application.Results;
using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Metrics.Infrastructure.Services;

public class UserTitleService : IUserTitleService
{
    public readonly MetricsDbContext _context;
    public readonly ILogger<UserTitleService> _logger;
    public readonly IUserTitleRepository _userTitleRepository;

    public UserTitleService(
        MetricsDbContext context,
        ILogger<UserTitleService> logger,
        IUserTitleRepository userTitleRepository)
    {
        _context = context;
        _logger = logger;
        _userTitleRepository = userTitleRepository;
    }

    public async Task<UserTitle> CreateAsync(UserTitle entity)
    {
        try
        {
            _userTitleRepository.Create(entity);
            await _context.SaveChangesAsync();

            return entity;
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

    public async Task<IEnumerable<UserTitle>> FindAllAsync()
    {
        var query = await _userTitleRepository
            .FindAllAsync();

        // return query
        // .Where(t => t.TitleName != "Admin");
        return query ?? [];
    }

    public async Task<ResultT<List<UserGroupDto>>> FindAll_Async()
    {
        try
        {
            var groups = await _userTitleRepository.FindAllAsync();
            var data = groups.Select(g => g.MapToDto()).ToList();
            return ResultT<List<UserGroupDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError("Unexpected error occured at fetching user groups. {msg}", ex.Message);
            return ResultT<List<UserGroupDto>>.Fail("Failed to load user groups.", ErrorType.UnexpectedError);
        }
    }

    public async Task<UserTitle?> FindByIdAsync(long id)
    {
        return await _userTitleRepository.FindByIdAsync(id);
    }
}
