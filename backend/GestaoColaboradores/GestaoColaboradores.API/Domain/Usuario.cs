using GestaoColaboradores.API.Services.Common.Exceptions;

namespace GestaoColaboradores.API.Domain;

public class Usuario : BaseEntity
{
    public string Codigo { get; private set; } 
    public string Login { get; private set; }
    public string Senha { get; private set; } 
    public Status Status { get; private set; }
    public Colaborador? Colaborador { get; private set; }

    // Construtor privado — exigido pelo EF Core para materializar a entidade
    // via reflection, mas impede criação fora do factory method abaixo.
    private Usuario()
    {
    }

    public Usuario(string login, string senhaHash, string codigo)
    {
        if (string.IsNullOrWhiteSpace(login))
        {
            throw new BadRequestException("Login é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new BadRequestException("Código é obrigatório.");
        }

        Login = login;
        Senha = senhaHash;
        Codigo = codigo;
        Status = Status.Ativo;
        CreatedAt = DateTime.UtcNow;
    }

    public void AtualizarSenha(string senhaHash)
    {
        Senha = senhaHash;
        MarcarComoAtualizado();
    }

    public void AtualizarStatus(Status status)
    {
        Status = status;
        MarcarComoAtualizado();
    }

    private void MarcarComoAtualizado() => UpdatedAt = DateTime.UtcNow;
}