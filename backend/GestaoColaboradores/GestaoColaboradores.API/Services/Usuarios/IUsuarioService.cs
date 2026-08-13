using GestaoColaboradores.API.Domain;

namespace GestaoColaboradores.API.Services.Usuarios;

public interface IUsuarioService
{
    Task<UsuarioResponse> CreateAsync(CreateUsuarioRequest request, CancellationToken ct);
    Task<UsuarioResponse> UpdateAsync(int id, UpdateUsuarioRequest request, CancellationToken ct);
    Task<List<UsuarioListItemResponse>> ListAsync(string? status, CancellationToken ct);
}
