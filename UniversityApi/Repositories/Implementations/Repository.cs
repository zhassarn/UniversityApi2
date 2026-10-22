using Microsoft.EntityFrameworkCore;
using UniversityApi.Data;
using UniversityApi.Repositories.Interfaces;

namespace UniversityApi.Repositories.Implementations;

public abstract class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<T> Set;

    protected Repository(ApplicationDbContext context)
    {
        Context = context;
        Set = context.Set<T>();
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync()
        => await Set.AsNoTracking().ToListAsync();

    public virtual async Task<T?> GetByIdAsync(int id)
        => await Set.FindAsync(id);

    public virtual async Task<bool> ExistsAsync(int id)
        => await Set.FindAsync(id) is not null;

    public virtual async Task<T> AddAsync(T entity)
    {
        await Set.AddAsync(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    // Entity GetByIdAsync арқылы алынған (tracked) болғандықтан, SaveChanges жеткілікті
    public virtual async Task UpdateAsync(T entity)
        => await Context.SaveChangesAsync();

    public virtual async Task DeleteAsync(T entity)
    {
        Set.Remove(entity);
        await Context.SaveChangesAsync();
    }
}
