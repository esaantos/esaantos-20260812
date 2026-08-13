using GestaoColaboradores.API.Domain;
using GestaoColaboradores.API.Infra;
using GestaoColaboradores.API.Infra.Security;
using Microsoft.EntityFrameworkCore;

namespace GestaoColaboradores.API.Services.Auth;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(AppDbContext db, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var usuario = await _db.Usuarios.SingleOrDefaultAsync(u => u.Login == request.Login);

        // Falha genérica para login inexistente, senha incorreta ou usuário inativo:
        // não deve ser possível distinguir o motivo pela resposta.
        if (usuario is null || usuario.Status != Status.Ativo)
        {
            return null;
        }

        if (!_passwordHasher.Verify(request.Senha, usuario.Senha))
        {
            return null;
        }

        var token = _jwtTokenGenerator.Generate(usuario);
        return new LoginResponse { Token = token.Token, ExpiresIn = token.ExpiresIn };
    }
}
