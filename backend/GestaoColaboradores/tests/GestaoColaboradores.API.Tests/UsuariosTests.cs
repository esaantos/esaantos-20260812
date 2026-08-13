using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GestaoColaboradores.API.Domain;
using GestaoColaboradores.API.Infra.Security;
using GestaoColaboradores.API.Services.Auth;
using GestaoColaboradores.API.Services.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace GestaoColaboradores.API.Tests;

public class UsuariosTests : IClassFixture<CustomWebApplicationFactory>
{
    private const string AdminLogin = "usuario.admin";
    private const string AdminSenha = "senha-admin-123";
    private const string CodigoUsuario = "USR0000004";

    private readonly HttpClient _client;

    public UsuariosTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        SeedAdminAsync(factory).GetAwaiter().GetResult();
        AuthenticateAsync().GetAwaiter().GetResult();
    }

    private static async Task SeedAdminAsync(CustomWebApplicationFactory factory)
    {
        var hasher = new BCryptPasswordHasher();

        await factory.SeedAsync(async db =>
        {
            if (await db.Usuarios.AnyAsync(u => u.Login == AdminLogin))
            {
                return;
            }

            db.Usuarios.Add( new Usuario(AdminLogin, hasher.Hash(AdminSenha), CodigoUsuario));
        });
    }

    private async Task AuthenticateAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { login = AdminLogin, senha = AdminSenha });
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.Token);
    }

    [Fact]
    public async Task Create_ComDadosValidos_Retorna201SemExporSenha()
    {
        var response = await _client.PostAsJsonAsync("/api/usuarios", new
        {
            login = "novo.usuario",
            senha = "senha123"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var raw = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("senha", raw, StringComparison.OrdinalIgnoreCase);

        var body = await response.Content.ReadFromJsonAsync<UsuarioResponse>();
        Assert.NotNull(body);
        Assert.Matches(@"^USR-\d{6}$", body!.Codigo);
        Assert.Equal("novo.usuario", body.Login);
        Assert.Equal("Ativo", body.Status);
        Assert.True(body.Id > 0);
    }

    [Fact]
    public async Task Create_ComCampoObrigatorioAusente_Retorna400()
    {
        var response = await _client.PostAsJsonAsync("/api/usuarios", new
        {
            login = "sem.senha"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ComLoginDuplicado_Retorna409()
    {
        await _client.PostAsJsonAsync("/api/usuarios", new
        {
            login = "login.duplicado",
            senha = "senha123"
        });

        var response = await _client.PostAsJsonAsync("/api/usuarios", new
        {
            login = "login.duplicado",
            senha = "senha123"
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Update_AlterandoSenhaEStatus_Retorna200()
    {
        var created = await _client.PostAsJsonAsync("/api/usuarios", new
        {
            login = "usuario.update",
            senha = "senha123"
        });
        var criado = await created.Content.ReadFromJsonAsync<UsuarioResponse>();

        var response = await _client.PutAsJsonAsync($"/api/usuarios/{criado!.Id}", new
        {
            senha = "novaSenha123",
            status = "Inativo"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<UsuarioResponse>();
        Assert.NotNull(body);
        Assert.Equal("Inativo", body!.Status);
        Assert.Equal(criado.Codigo, body.Codigo);
        Assert.Equal(criado.Login, body.Login);
    }

    [Fact]
    public async Task Update_ComCampoNaoPermitido_Retorna400()
    {
        var created = await _client.PostAsJsonAsync("/api/usuarios", new
        {
            login = "usuario.campoinvalido",
            senha = "senha123"
        });
        var criado = await created.Content.ReadFromJsonAsync<UsuarioResponse>();

        var response = await _client.PutAsJsonAsync($"/api/usuarios/{criado!.Id}", new
        {
            status = "Inativo",
            login = "tentativa.alterar.login"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_ComUsuarioInexistente_Retorna404()
    {
        var response = await _client.PutAsJsonAsync("/api/usuarios/999999", new
        {
            status = "Inativo"
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ComStatusInvalido_Retorna400()
    {
        var created = await _client.PostAsJsonAsync("/api/usuarios", new
        {
            login = "usuario.statusinvalido",
            senha = "senha123"
        });
        var criado = await created.Content.ReadFromJsonAsync<UsuarioResponse>();

        var response = await _client.PutAsJsonAsync($"/api/usuarios/{criado!.Id}", new
        {
            status = "10"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task List_ComStatusInvalido_Retorna400()
    {
        var response = await _client.GetAsync("/api/usuarios?status=10");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task List_RetornaLoginEStatusSemSenha()
    {
        await _client.PostAsJsonAsync("/api/usuarios", new
        {
            login = "usuario.listagem",
            senha = "senha123"
        });

        var response = await _client.GetAsync("/api/usuarios");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var raw = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("senha123", raw, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("usuario.listagem", raw);
    }

    [Fact]
    public async Task List_ComFiltroStatus_RetornaSomenteUsuariosFiltrados()
    {
        await _client.PostAsJsonAsync("/api/usuarios", new
        {
            login = "usuario.filtro.ativo",
            senha = "senha123"
        });

        var criadoInativo = await _client.PostAsJsonAsync("/api/usuarios", new
        {
            login = "usuario.filtro.inativo",
            senha = "senha123"
        });
        var inativo = await criadoInativo.Content.ReadFromJsonAsync<UsuarioResponse>();
        await _client.PutAsJsonAsync($"/api/usuarios/{inativo!.Id}", new { status = "Inativo" });

        var response = await _client.GetAsync("/api/usuarios?status=Inativo");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<UsuarioListItemResponse>>();
        Assert.NotNull(body);
        Assert.Contains(body!, u => u.Login == "usuario.filtro.inativo");
        Assert.DoesNotContain(body, u => u.Login == "usuario.filtro.ativo");
        Assert.All(body, u => Assert.Equal("Inativo", u.Status));
    }
}
