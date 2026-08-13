using GestaoColaboradores.API.Domain;
using Microsoft.EntityFrameworkCore;

namespace GestaoColaboradores.API.Infra.Repositories;

public class ColaboradorRepository : BaseRepository<Colaborador>, IColaboradorRepository
{
    public ColaboradorRepository(AppDbContext db) : base(db)
    {
    }

    public Task<Colaborador?> GetByCodigoAsync(string codigo, CancellationToken ct) =>
        DbSet.SingleOrDefaultAsync(c => c.Codigo == codigo, ct);

    public Task<Colaborador?> GetByUsuarioIdAsync(int usuarioId, CancellationToken ct) =>
        DbSet.SingleOrDefaultAsync(c => c.UsuarioId == usuarioId, ct);

    public Task<Colaborador?> GetByIdWithUnidadeAsync(int id, CancellationToken ct) =>
        DbSet.Include(c => c.Unidade).SingleOrDefaultAsync(c => c.Id == id, ct);

    public Task<List<Colaborador>> ListWithUnidadeAsync(CancellationToken ct) =>
        DbSet.Include(c => c.Unidade)
            .OrderBy(c => c.Id)
            .ToListAsync(ct);
}

public interface IColaboradorRepository : IRepository<Colaborador>
{
    Task<Colaborador?> GetByCodigoAsync(string codigo, CancellationToken ct);
    Task<Colaborador?> GetByUsuarioIdAsync(int usuarioId, CancellationToken ct);
    Task<Colaborador?> GetByIdWithUnidadeAsync(int id, CancellationToken ct);
    Task<List<Colaborador>> ListWithUnidadeAsync(CancellationToken ct);
}
