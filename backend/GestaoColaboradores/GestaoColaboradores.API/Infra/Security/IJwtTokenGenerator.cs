using GestaoColaboradores.API.Domain;

namespace GestaoColaboradores.API.Infra.Security;

public record TokenResult(string Token, int ExpiresIn);

public interface IJwtTokenGenerator
{
    TokenResult Generate(Usuario usuario);
}
