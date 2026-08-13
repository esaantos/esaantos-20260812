using GestaoColaboradores.API.Domain;
using Microsoft.EntityFrameworkCore;

namespace GestaoColaboradores.API.Infra.Repositories;

public class BaseRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext Db;
    protected readonly DbSet<T> DbSet;

    public BaseRepository(AppDbContext db)
    {
        Db = db;
        DbSet = db.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default) => await DbSet.FindAsync(id, ct);

    public IQueryable<T> Query() => DbSet.AsQueryable();

    public async Task AddAsync(T entity, CancellationToken ct = default) => await DbSet.AddAsync(entity, ct);

    public void Remove(T entity) => DbSet.Remove(entity);

    public async Task<bool> SaveChangesAsync(CancellationToken ct = default) => await Db.SaveChangesAsync(ct) > 0;
}

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    IQueryable<T> Query();
    Task AddAsync(T entity, CancellationToken ct = default);
    void Remove(T entity);
    Task<bool> SaveChangesAsync(CancellationToken ct = default);
}