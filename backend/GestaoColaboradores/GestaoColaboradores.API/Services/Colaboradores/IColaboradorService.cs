namespace GestaoColaboradores.API.Services.Colaboradores;

public interface IColaboradorService
{
    Task<ColaboradorResponse> CreateAsync(CreateColaboradorRequest request, CancellationToken ct);
    Task<ColaboradorResponse> UpdateAsync(int id, UpdateColaboradorRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
    Task<List<ColaboradorListItemResponse>> ListAsync(CancellationToken ct);
}
