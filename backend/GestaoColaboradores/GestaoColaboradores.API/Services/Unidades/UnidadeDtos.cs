namespace GestaoColaboradores.API.Services.Unidades;

public record CreateUnidadeRequest(
    string Nome);

public record UpdateUnidadeStatusRequest(
    string Status);

public record UnidadeResponse(
    int Id,
    string CodigoUnidade,
    string Nome,
    string Status
);


public record ColaboradorResumoResponse(
    string Codigo,
    string Nome
);

public record UnidadeListItemResponse(
    int Id,
    string CodigoUnidade,
    string Nome,
    string Status,
    List<ColaboradorResumoResponse> Colaboradores
);
