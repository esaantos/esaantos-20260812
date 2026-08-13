using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GestaoColaboradores.API.Domain;
using GestaoColaboradores.API.Infra.Security;
using GestaoColaboradores.API.Services.Auth;
using GestaoColaboradores.API.Services.Unidades;
using Microsoft.EntityFrameworkCore;

namespace GestaoColaboradores.API.Tests;

public class UnidadesTests : IClassFixture<CustomWebApplicationFactory>
{
    private const string AdminLogin = "unidades.admin";
    private const string AdminSenha = "senha-admin-123";

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public UnidadesTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
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

            db.Usuarios.Add(new Usuario(AdminLogin, hasher.Hash(AdminSenha), "USR0000003"));

        });
    }

    private async Task AuthenticateAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { login = AdminLogin, senha = AdminSenha });
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.Token);
    }

    [Fact]
    public async Task Create_ComDadosValidos_Retorna201ComStatusAtiva()
    {
        var response = await _client.PostAsJsonAsync("/api/unidades", new
        {
            nome = "Filial Teste"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<UnidadeResponse>();
        Assert.NotNull(body);
        Assert.Matches(@"^UNI-\d{6}$", body!.CodigoUnidade);
        Assert.Equal("Filial Teste", body.Nome);
        Assert.Equal("Ativo", body.Status);
        Assert.True(body.Id > 0);
    }

    [Fact]
    public async Task Create_ComCampoObrigatorioAusente_Retorna400()
    {
        var response = await _client.PostAsJsonAsync("/api/unidades", new
        {
            nome = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Inativar_UnidadeAtiva_Retorna200ComStatusInativa()
    {
        var created = await _client.PostAsJsonAsync("/api/unidades", new
        {
            nome = "Filial a Inativar"
        });
        var criada = await created.Content.ReadFromJsonAsync<UnidadeResponse>();

        var response = await _client.PutAsJsonAsync($"/api/unidades/{criada!.Id}", new { status = "Inativo" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<UnidadeResponse>();
        Assert.NotNull(body);
        Assert.Equal("Inativo", body!.Status);
        Assert.Equal(criada.CodigoUnidade, body.CodigoUnidade);
    }

    [Fact]
    public async Task Reativar_UnidadeInativa_Retorna200ComStatusAtivo()
    {
        var created = await _client.PostAsJsonAsync("/api/unidades", new
        {
            nome = "Filial a Reativar"
        });
        var criada = await created.Content.ReadFromJsonAsync<UnidadeResponse>();

        var inativada = await _client.PutAsJsonAsync($"/api/unidades/{criada!.Id}", new { status = "Inativo" });
        Assert.Equal(HttpStatusCode.OK, inativada.StatusCode);

        var response = await _client.PutAsJsonAsync($"/api/unidades/{criada.Id}", new { status = "Ativo" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<UnidadeResponse>();
        Assert.NotNull(body);
        Assert.Equal("Ativo", body!.Status);
    }

    [Fact]
    public async Task Inativar_ComStatusInvalido_Retorna400()
    {
        var created = await _client.PostAsJsonAsync("/api/unidades", new
        {
            nome = "Filial Status Invalido"
        });
        var criada = await created.Content.ReadFromJsonAsync<UnidadeResponse>();

        var response = await _client.PutAsJsonAsync($"/api/unidades/{criada!.Id}", new { status = "10" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Inativar_UnidadeInexistente_Retorna404()
    {
        var response = await _client.PutAsJsonAsync("/api/unidades/999999", new { status = "Inativo" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task List_RetornaUnidadesComColaboradoresRelacionados()
    {
        var hasher = new BCryptPasswordHasher();

        await _factory.SeedAsync(async db =>
        {
            var unidade = new Unidade("UN-LIST", "Filial Listagem");

            db.Unidades.Add(unidade);

            var usuario = new Usuario("colaborador.listagem", hasher.Hash("senha123"), "USR0000005");

            db.Usuarios.Add(usuario);

            await db.SaveChangesAsync();

            db.Colaboradores.Add(new Colaborador("C-LIST", "Colaborador Listagem", unidade.Id, usuario.Id));
        });

        var response = await _client.GetAsync("/api/unidades");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<UnidadeListItemResponse>>();
        Assert.NotNull(body);

        var unidadeListada = Assert.Single(body!, u => u.CodigoUnidade == "UN-LIST");
        var colaborador = Assert.Single(unidadeListada.Colaboradores);
        Assert.Equal("C-LIST", colaborador.Codigo);
        Assert.Equal("Colaborador Listagem", colaborador.Nome);
    }
}
