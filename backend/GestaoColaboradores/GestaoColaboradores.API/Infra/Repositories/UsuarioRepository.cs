using GestaoColaboradores.API.Domain;
using Microsoft.EntityFrameworkCore;

namespace GestaoColaboradores.API.Infra.Repositories;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> GetByLoginAsync(string login, CancellationToken ct = default);
    Task<Usuario?> GetByCodigoAsync(string codigo, CancellationToken ct = default);
}

public class UsuarioRepository : BaseRepository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(AppDbContext db) : base(db)
    {
    }

    public Task<Usuario?> GetByLoginAsync(string login, CancellationToken ct = default) =>
        DbSet.SingleOrDefaultAsync(u => u.Login == login, ct);

    public Task<Usuario?> GetByCodigoAsync(string codigo, CancellationToken ct = default) =>
        DbSet.SingleOrDefaultAsync(u => u.Codigo == codigo, ct);
}
