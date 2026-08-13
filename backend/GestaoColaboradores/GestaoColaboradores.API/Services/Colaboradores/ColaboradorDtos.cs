namespace GestaoColaboradores.API.Services.Colaboradores;

public record CreateColaboradorRequest(
    string Nome,
    int UnidadeId,
    int UsuarioId
);
public record UpdateColaboradorRequest(
    string? Nome,
    int? UnidadeId
);
public record UnidadeResumoResponse(
    int Id,
    string Nome
);

public record ColaboradorResponse(
    int Id,
    string Codigo,
    string Nome,
    UnidadeResumoResponse Unidade,
    int UsuarioId
);

public record ColaboradorListItemResponse(
    int Id,
    string Codigo,
    string Nome,
    UnidadeResumoResponse Unidade
);
