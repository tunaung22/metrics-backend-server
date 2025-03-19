using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Infrastructure.Repositories.IRepositories;

public interface IGenericRepository<T> where T : class
{
    //Task<IEnumerable<T?>> FindAsync(Expression<Func<T, bool>> expression);
    Task<T?> FindByIdAsync(long id);
    IQueryable<T?> Find(Expression<Func<T, bool>> expression);
    IQueryable<T> FindAny(Expression<Func<T, bool>> expression);
    //Task<IQueryable<T?>> FindById(long id);
    Task<T> CreateAsync(T entity);
    Task<EntityEntry<T>> CreateAsync2(T entity);
    void Update(T entity);
    void Delete(T entity);
    IQueryable<T> FindAll();
    Task<IEnumerable<T>> FindAllAsync();

}
