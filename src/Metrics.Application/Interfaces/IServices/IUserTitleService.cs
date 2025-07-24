using Metrics.Application.Domains;
using System;

namespace Metrics.Application.Interfaces.IServices;

public interface IUserTitleService
{
    Task<UserTitle> CreateAsync(UserTitle entity);
    // Task<UserTitle> UpdateAsync(string UserTitleCode, UserTitle entity);
    // Task<bool> DeleteAsync(string titleCode);
    Task<UserTitle?> FindByIdAsync(long id);
    // Task<UserTitle> FindByUserTitleCodeAsync(string titleCode);
    // Task<UserTitle?> FindByUserTitleNameAsync(string titleName);
    Task<IEnumerable<UserTitle>> FindAllAsync();
    // Task<IEnumerable<UserTitle>> FindAllAsync(int pageNumber = 1, int pageSize = 20);
    // Task<long> FindCountAsync();
}
