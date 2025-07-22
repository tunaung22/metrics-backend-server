using Metrics.Application.Domains;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Application.Interfaces.IServices;
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
        try
        {
            var query = await _userTitleRepository
                .FindAllAsync();

            // return query
            // .Where(t => t.TitleName != "Admin");
            return query ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying departments.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<UserTitle> FindByIdAsync(long id)
    {
        try
        {
            var userTitle = await _userTitleRepository.FindByIdAsync(id);
            if (userTitle == null)
                throw new NotFoundException($"User Title with id {id} not found.");

            return userTitle;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying UserTitle by id.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }
}
