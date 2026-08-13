using GestaoColaboradores.API.Domain;
using GestaoColaboradores.API.Infra.Repositories;
using GestaoColaboradores.API.Infra.Security;
using GestaoColaboradores.API.Services.Common;
using GestaoColaboradores.API.Services.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GestaoColaboradores.API.Services.Usuarios;

public class UsuarioService(
    IUsuarioRepository repository,
    IPasswordHasher passwordHasher,
    ICodigoSequencialGenerator codigoGenerator) : IUsuarioService
{
    private const int SenhaTamanhoMinimo = 8;
    private const string SequenceName = "usuario_codigo_seq";
    private const string Prefixo = "USR";

    public async Task<UsuarioResponse> CreateAsync(CreateUsuarioRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Senha))
        {
            throw new BadRequestException("Login e Senha são obrigatórios.");
        }

        if (await repository.GetByLoginAsync(request.Login, ct) is not null)
        {
            throw new ConflictException("Login já cadastrado.");
        }

        ValidarPoliticaSenha(request.Senha);

        var senhaHash = passwordHasher.Hash(request.Senha);

        var codigo = await codigoGenerator.ProximoAsync(SequenceName, Prefixo, ct);

        var usuario = new Usuario(request.Login, senhaHash, codigo);

        await repository.AddAsync(usuario, ct);

        try
        {
            await repository.SaveChangesAsync(ct);    

        }
        catch (DbUpdateException ex) when (DbExceptionTranslator.IsUniqueViolation(ex))
        {
            throw new ConflictException("Login já cadastrado.");
        }

        return ToResponse(usuario);
    }

    public async Task<UsuarioResponse> UpdateAsync(int id, UpdateUsuarioRequest request, CancellationToken ct = default)
    {
        if (request.Senha is null && request.Status is null)
        {
            throw new BadRequestException("Informe ao menos Senha ou Status para atualização.");
        }

        var usuario = await repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Usuário não encontrado.");

        if (request.Status is not null)
        {
            usuario.AtualizarStatus(ParseStatusOuFalha(request.Status));
        }

        if (request.Senha is not null)
        {
            ValidarPoliticaSenha(request.Senha);
            usuario.AtualizarSenha(passwordHasher.Hash(request.Senha));
        }

        await repository.SaveChangesAsync(ct);

        return ToResponse(usuario);
    }

    public async Task<List<UsuarioListItemResponse>> ListAsync(string? status, CancellationToken ct = default)
    {
        var query = repository.Query();
        if (status is not null)
        {
            var statusFiltro = ParseStatusOuFalha(status);
            query = query.Where(u => u.Status == statusFiltro);
        }

        return await query
            .OrderBy(u => u.Id)
            .Select(u => new UsuarioListItemResponse( u.Id, u.Login, u.Status.ToString() ))
            .ToListAsync(ct);
    }

    private static void ValidarPoliticaSenha(string senha)
    {
        if (senha.Length < SenhaTamanhoMinimo)
        {
            throw new BadRequestException($"Senha deve ter ao menos {SenhaTamanhoMinimo} caracteres.");
        }
    }

    private static Status ParseStatusOuFalha(string status) => status.Trim().ToLowerInvariant() switch
    {
        "ativo" => Status.Ativo,
        "inativo" => Status.Inativo,
        _ => throw new BadRequestException("Status inválido. Use 'Ativo' ou 'Inativo'.")
    };

    private static UsuarioResponse ToResponse(Usuario usuario)
        => new(usuario.Id, usuario.Codigo, usuario.Login, usuario.Status.ToString());
}