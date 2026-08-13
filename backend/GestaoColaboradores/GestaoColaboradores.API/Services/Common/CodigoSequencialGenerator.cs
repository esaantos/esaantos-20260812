using GestaoColaboradores.API.Infra;
using Microsoft.EntityFrameworkCore;

namespace GestaoColaboradores.API.Services.Common;

public class CodigoSequencialGenerator(AppDbContext context) : ICodigoSequencialGenerator
{
    private const int TotalDigitos = 6;

    public async Task<string> ProximoAsync(string sequenceName, string prefixo, CancellationToken ct = default)
    {
        // nextval() do Postgres retorna bigint — usar long evita erro de cast quando a sequence passar de int.MaxValue (improvável aqui, mas seguro).
        var proximo = await context.Database
            .SqlQueryRaw<long>($"SELECT nextval('{sequenceName}') AS \"Value\"")
            .SingleAsync(ct);

        return Formatar(proximo, prefixo);
    }

    public static string Formatar(long numero, string prefixo) =>
        $"{prefixo}-{numero.ToString().PadLeft(TotalDigitos, '0')}";
}
public interface ICodigoSequencialGenerator
{
    Task<string> ProximoAsync(string sequenceName, string prefixo, CancellationToken ct = default);
}