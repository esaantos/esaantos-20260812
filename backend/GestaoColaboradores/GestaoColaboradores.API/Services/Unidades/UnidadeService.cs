using GestaoColaboradores.API.Domain;
using GestaoColaboradores.API.Infra.Repositories;
using GestaoColaboradores.API.Services.Common;
using GestaoColaboradores.API.Services.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GestaoColaboradores.API.Services.Unidades;

public class UnidadeService(
    IUnidadeRepository repository,
    ICodigoSequencialGenerator codigoGenerator) : IUnidadeService
{
    private const string SequenceName = "unidade_codigo_seq";
    private const string Prefixo = "UNI";

    public async Task<UnidadeResponse> CreateAsync(CreateUnidadeRequest request, CancellationToken ct)
    {
        var codigo = await codigoGenerator.ProximoAsync(SequenceName, Prefixo, ct);

        var unidade = new Unidade(codigo, request.Nome);

        await repository.AddAsync(unidade);

        try
        {
            await repository.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (DbExceptionTranslator.IsUniqueViolation(ex))
        {
            throw new ConflictException("Código da unidade já cadastrado.");
        }

        return ToResponse(unidade);
    }

    public async Task<UnidadeResponse> UpdateAsync(int id, UpdateUnidadeStatusRequest request, CancellationToken ct)
    {
        var unidade = await repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Unidade não encontrada.");

        unidade.AtualizarStatusUnidade(ParseStatusOuFalha(request.Status));

        await repository.SaveChangesAsync();

        return ToResponse(unidade);
    }

    public async Task<List<UnidadeListItemResponse>> ListAsync(CancellationToken ct)
    {
        var unidades = await repository.ListWithColaboradoresAsync(ct);

        return unidades.Select(u => new UnidadeListItemResponse(
        
            u.Id,
            u.CodigoUnidade,
            u.Nome,
            u.Status.ToString(),
            u.Colaboradores
                .OrderBy(c => c.Nome)
                .Select(c => new ColaboradorResumoResponse(c.Codigo, c.Nome))
                .ToList()
        )).ToList();
    }

    private static Status ParseStatusOuFalha(string status) => status.Trim().ToLowerInvariant() switch
    {
        "ativo" => Status.Ativo,
        "inativo" => Status.Inativo,
        _ => throw new BadRequestException("Status inválido. Use 'Ativo' ou 'Inativo'.")
    };

    private static UnidadeResponse ToResponse(Unidade unidade) => new(
        unidade.Id,
        unidade.CodigoUnidade,
        unidade.Nome,
        unidade.Status.ToString()
    );
}
