using Metrics.Infrastructure.Data;
using Metrics.Infrastructure.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly MetricsDbContext _context;
    protected readonly DbSet<T> _dbSet;


    public GenericRepository(MetricsDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }


    //public async Task<IEnumerable<T?>> FindAsync(Expression<Func<T, bool>> expression)
    //{
    //    return await _context.Set<T>().Where(expression).ToListAsync();
    //}

    public async Task<T?> FindByIdAsync(long id)
    {
        return await _dbSet.FindAsync(id);
    }

    // TO REMOVE
    public IQueryable<T?> Find(Expression<Func<T, bool>> expression)
    {
        return _dbSet.Where(expression);
    }

    public IQueryable<T> FindAny(Expression<Func<T, bool>> expression)
    {
        return _dbSet.Where(expression);
    }

    public async Task<T> CreateAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public async Task<EntityEntry<T>> CreateAsync2(T entity)
    {
        return await _dbSet.AddAsync(entity);
    }

    public void Update(T entity)
    {
        _dbSet.Attach(entity);
        _dbSet.Entry(entity).State = EntityState.Modified;
        // _dbSet.Update(entity);
    }

    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }
    public IQueryable<T> FindAll()
    {
        return _dbSet;
    }

    public async Task<IEnumerable<T>> FindAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

}
