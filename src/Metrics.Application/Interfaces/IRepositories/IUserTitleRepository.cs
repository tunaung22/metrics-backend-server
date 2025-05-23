using Metrics.Application.Domains;

namespace Metrics.Application.Interfaces.IRepositories;

public interface IUserTitleRepository
{
    void Create(UserTitle entity);
    // void Update(UserTitle entity);
    // void Delete(UserTitle entity);
    Task<IEnumerable<UserTitle>> FindAllAsync();
    // Task<IEnumerable<UserTitle>> FindAllAsync(int pageNumber, int pageSize);
    Task<UserTitle?> FindByIdAsync(long id);
    // Task<UserTitle?> FindByTitleCodeAsync(string titleCode);
    Task<UserTitle?> FindByTitleNameAsync(string titleName);
    // Task<bool> UserTitleExistsAsync(string titleCode);
    // Task<long> FindCountAsync();
}
