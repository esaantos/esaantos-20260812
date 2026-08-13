using GestaoColaboradores.API.Services.Common.Exceptions;

namespace GestaoColaboradores.API.Domain;

public class Colaborador : BaseEntity
{
    public string Codigo { get; private set; }
    public string Nome { get; private set; } 

    public int UnidadeId { get; private set; }
    public int UsuarioId { get; private set; }
    public Unidade Unidade { get; private set; } = null!;
    public Usuario Usuario { get; private set; } = null!;

    public Colaborador(string codigo, string nome, int unidadeId, int usuarioId)
    {
        if (string.IsNullOrWhiteSpace(codigo) ||
            string.IsNullOrWhiteSpace(nome) ||
            unidadeId <= 0 ||
            usuarioId <= 0)
        {
            throw new BadRequestException("Codigo, Nome, UnidadeId e UsuarioId são obrigatórios.");
        }

        Codigo = codigo;
        Nome = nome;
        UnidadeId = unidadeId;
        UsuarioId = usuarioId;
        CreatedAt = DateTime.UtcNow;
    }

    internal void UpdateName(string nome)
    {
        Nome = nome;
    }

    internal void UpdateUnidade(Unidade novaUnidade)
    {
        Unidade = novaUnidade;
    }

    internal void MarcarComoAtualizado() => UpdatedAt = DateTime.UtcNow;
}
