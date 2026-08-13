using GestaoColaboradores.API.Domain;
using Microsoft.EntityFrameworkCore;

namespace GestaoColaboradores.API.Infra.Repositories;

public class UnidadeRepository : BaseRepository<Unidade>, IUnidadeRepository
{
    public UnidadeRepository(AppDbContext db) : base(db)
    {
    }

    public Task<Unidade?> GetByCodigoUnidadeAsync(string codigoUnidade, CancellationToken ct) =>
        DbSet.SingleOrDefaultAsync(u => u.CodigoUnidade == codigoUnidade, ct);

    public Task<List<Unidade>> ListWithColaboradoresAsync(CancellationToken ct) =>
        DbSet.Include(u => u.Colaboradores)
            .OrderBy(u => u.CodigoUnidade)
            .ToListAsync(ct);
}

public interface IUnidadeRepository : IRepository<Unidade>
{
    Task<Unidade?> GetByCodigoUnidadeAsync(string codigoUnidade, CancellationToken ct);
    Task<List<Unidade>> ListWithColaboradoresAsync(CancellationToken ct);
}