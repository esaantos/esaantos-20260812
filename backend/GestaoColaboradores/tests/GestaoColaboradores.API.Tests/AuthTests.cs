using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GestaoColaboradores.API.Domain;
using GestaoColaboradores.API.Infra.Security;
using GestaoColaboradores.API.Services.Auth;
using Microsoft.EntityFrameworkCore;

namespace GestaoColaboradores.API.Tests;

public class AuthTests : IClassFixture<CustomWebApplicationFactory>
{
    private const string LoginAtivo = "usuario.ativo";
    private const string SenhaAtivo = "senha-correta-123";
    private const string LoginInativo = "usuario.inativo";
    private const string SenhaInativo = "senha-correta-456";
    private const string CodigoUsuarioAtivo = "USR0000001";
    private const string CodigoUsuarioInativo = "USR0000002";

    private readonly HttpClient _client;

    public AuthTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        SeedUsuariosAsync(factory).GetAwaiter().GetResult();
    }

    private static async Task SeedUsuariosAsync(CustomWebApplicationFactory factory)
    {
        var hasher = new BCryptPasswordHasher();

        await factory.SeedAsync(async db =>
        {
            if (await db.Usuarios.AnyAsync(u => u.Login == LoginAtivo))
            {
                return;
            }

            db.Usuarios.Add(new Usuario(LoginAtivo, hasher.Hash(SenhaAtivo), CodigoUsuarioAtivo));

            var usuarioInativo = new Usuario(LoginInativo, hasher.Hash(SenhaInativo), CodigoUsuarioInativo);
            usuarioInativo.AtualizarStatus(Status.Inativo);
            db.Usuarios.Add(usuarioInativo);
        });
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_Retorna200ComToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { login = LoginAtivo, senha = SenhaAtivo });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body!.Token));
        Assert.Equal(3600, body.ExpiresIn);
    }

    [Fact]
    public async Task Login_ComSenhaIncorreta_Retorna401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { login = LoginAtivo, senha = "senha-errada" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ComUsuarioInativo_Retorna401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { login = LoginInativo, senha = SenhaInativo });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_SemToken_Retorna401()
    {
        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_ComTokenInvalido_Retorna401()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "token-invalido");

        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
