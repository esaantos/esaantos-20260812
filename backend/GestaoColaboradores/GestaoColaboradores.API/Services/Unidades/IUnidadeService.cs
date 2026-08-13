namespace GestaoColaboradores.API.Services.Unidades;

public interface IUnidadeService
{
    Task<UnidadeResponse> CreateAsync(CreateUnidadeRequest request, CancellationToken ct);
    Task<UnidadeResponse> UpdateAsync(int id, UpdateUnidadeStatusRequest request, CancellationToken ct);
    Task<List<UnidadeListItemResponse>> ListAsync(CancellationToken ct);
}
