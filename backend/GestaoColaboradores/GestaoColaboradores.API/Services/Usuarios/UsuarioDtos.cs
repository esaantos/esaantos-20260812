using System.Text.Json.Serialization;

namespace GestaoColaboradores.API.Services.Usuarios;

public record CreateUsuarioRequest(string Login, string Senha);

// Atualização aceita somente Senha e Status (regra de negócio) — qualquer outro
// campo no payload deve ser rejeitado com 400, não silenciosamente ignorado.
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public record UpdateUsuarioRequest(string? Senha, string? Status);

public record UsuarioResponse(int Id, string Codigo, string Login, string Status);

public record UsuarioListItemResponse(int Id, string Login, string Status);
