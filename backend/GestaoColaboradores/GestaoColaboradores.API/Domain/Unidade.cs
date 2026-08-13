using GestaoColaboradores.API.Services.Common.Exceptions;

namespace GestaoColaboradores.API.Domain;

public class Unidade : BaseEntity
{
    public string CodigoUnidade { get; private set; }
    public string Nome { get; private set; }
    public Status Status { get; private set; }

    public ICollection<Colaborador> Colaboradores { get; private set; } = [];

    private Unidade() { }

    public Unidade(string codigoUnidade, string nome)
    {
        if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(codigoUnidade))
            throw new BadRequestException("Nome e código da unidade são obrigatórios.");

        CodigoUnidade = codigoUnidade;
        Nome = nome;
        Status = Status.Ativo;
        CreatedAt = DateTime.UtcNow;
    }

    public void AtualizarStatusUnidade(Status status)
    {
        Status = status;
        MarcarComoAtualizado();
    }

    private void MarcarComoAtualizado() => UpdatedAt = DateTime.UtcNow;
}
