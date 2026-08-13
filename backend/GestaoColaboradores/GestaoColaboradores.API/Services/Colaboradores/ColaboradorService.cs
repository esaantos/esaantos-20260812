using GestaoColaboradores.API.Domain;
using GestaoColaboradores.API.Infra.Repositories;
using GestaoColaboradores.API.Services.Common;
using GestaoColaboradores.API.Services.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GestaoColaboradores.API.Services.Colaboradores;

public class ColaboradorService(
    IColaboradorRepository colaboradorRepository,
    IUsuarioRepository usuarioRepository,
    IUnidadeRepository unidadeRepository, 
    ICodigoSequencialGenerator codigoGenerator) : IColaboradorService
{
    private const string SequenceName = "colaborador_codigo_seq";
    private const string Prefixo = "COL";

    public async Task<ColaboradorResponse> CreateAsync(CreateColaboradorRequest request, CancellationToken ct)
    {
        var usuario = await usuarioRepository.GetByIdAsync(request.UsuarioId, ct)
            ?? throw new NotFoundException("Usuario não encontrado.");

        if (await colaboradorRepository.GetByUsuarioIdAsync(usuario.Id, ct) is not null)
        {
            throw new ConflictException("Usuario já vinculado a outro colaborador.");
        }

        var unidade = await unidadeRepository.GetByIdAsync(request.UnidadeId, ct)
            ?? throw new NotFoundException("Unidade não encontrada.");

        // Unidade inativa não pode receber novos colaboradores.
        if (unidade.Status != Status.Ativo)
        {
            throw new UnprocessableEntityException("Unidade inativa não pode receber novos colaboradores.");
        }

        var codigo = await codigoGenerator.ProximoAsync(SequenceName, Prefixo, ct);

        var colaborador = new Colaborador(codigo, request.Nome, unidade.Id, usuario.Id);

        await colaboradorRepository.AddAsync(colaborador);

        try
        {
            await colaboradorRepository.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (DbExceptionTranslator.IsUniqueViolation(ex))
        {
            var mensagem = DbExceptionTranslator.GetConstraintName(ex) == "IX_Colaboradores_UsuarioId"
                ? "Usuario já vinculado a outro colaborador."
                : "Codigo já cadastrado.";
            throw new ConflictException(mensagem);
        }

        return ToResponse(colaborador, unidade);
    }

    public async Task<ColaboradorResponse> UpdateAsync(int id, UpdateColaboradorRequest request, CancellationToken ct)
    {
        if (request.Nome is null && request.UnidadeId is null)
        {
            throw new BadRequestException("Informe ao menos Nome ou UnidadeId para atualização.");
        }

        var colaborador = await colaboradorRepository.GetByIdWithUnidadeAsync(id, ct)
            ?? throw new NotFoundException("Colaborador não encontrado.");

        if (request.Nome is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Nome))
            {
                throw new BadRequestException("Nome não pode ser vazio.");
            }

            colaborador.UpdateName(request.Nome);
        }

        if (request.UnidadeId is not null)
        {
            var novaUnidade = await unidadeRepository.GetByIdAsync(request.UnidadeId.Value, ct)
                ?? throw new NotFoundException("Unidade não encontrada.");

            // Mesma regra do cadastro: não pode mover colaborador para uma unidade inativa.
            if (novaUnidade.Status != Status.Ativo)
            {
                throw new UnprocessableEntityException("Unidade inativa não pode receber novos colaboradores.");
            }

            colaborador.UpdateUnidade(novaUnidade);
        }

        colaborador.MarcarComoAtualizado();
        await colaboradorRepository.SaveChangesAsync();

        return ToResponse(colaborador, colaborador.Unidade);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var colaborador = await colaboradorRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Colaborador não encontrado.");

        colaboradorRepository.Remove(colaborador);
        await colaboradorRepository.SaveChangesAsync();
    }

    public async Task<List<ColaboradorListItemResponse>> ListAsync(CancellationToken ct)
    {
        var colaboradores = await colaboradorRepository.ListWithUnidadeAsync(ct);

        return colaboradores.Select(c => new ColaboradorListItemResponse
        (
            c.Id,
            c.Codigo,
            c.Nome,
            new UnidadeResumoResponse (c.Unidade.Id, c.Unidade.Nome)
        )).ToList();
    }

    private static ColaboradorResponse ToResponse(Colaborador colaborador, Unidade unidade) => new(
        colaborador.Id,
        colaborador.Codigo,
        colaborador.Nome,
        new UnidadeResumoResponse(unidade.Id, unidade.Nome),
        colaborador.UsuarioId
    );
}
