using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GestaoColaboradores.API.Domain;
using GestaoColaboradores.API.Infra.Security;
using GestaoColaboradores.API.Services.Auth;
using GestaoColaboradores.API.Services.Colaboradores;
using GestaoColaboradores.API.Services.Unidades;
using GestaoColaboradores.API.Services.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace GestaoColaboradores.API.Tests;

public class ColaboradoresTests : IClassFixture<CustomWebApplicationFactory>
{
    private const string AdminLogin = "colaboradores.admin";
    private const string AdminSenha = "senha-admin-123";

    private readonly HttpClient _client;

    public ColaboradoresTests(CustomWebApplicationFactory factory)
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

            db.Usuarios.Add(new Usuario(AdminLogin, hasher.Hash(AdminSenha), "USR0000003"));
        });
    }

    private async Task AuthenticateAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { login = AdminLogin, senha = AdminSenha });
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.Token);
    }

    private async Task<int> CriarUsuarioAsync(string login)
    {
        var response = await _client.PostAsJsonAsync("/api/usuarios", new
        {
            login,
            senha = "senha123"
        });
        var body = await response.Content.ReadFromJsonAsync<UsuarioResponse>();
        return body!.Id;
    }

    private async Task<int> CriarUnidadeAsync(string nome)
    {
        var response = await _client.PostAsJsonAsync("/api/unidades", new { nome });
        var body = await response.Content.ReadFromJsonAsync<UnidadeResponse>();
        return body!.Id;
    }

    private async Task<int> CriarUnidadeInativaAsync(string nome)
    {
        var unidadeId = await CriarUnidadeAsync(nome);
        await _client.PutAsJsonAsync($"/api/unidades/{unidadeId}", new { status = "Inativo" });
        return unidadeId;
    }

    [Fact]
    public async Task Create_ComDadosValidos_Retorna201ComUnidadeAssociada()
    {
        var usuarioId = await CriarUsuarioAsync("colaborador.valido");
        var unidadeId = await CriarUnidadeAsync("Filial Colaborador");

        var response = await _client.PostAsJsonAsync("/api/colaboradores", new
        {
            nome = "Colaborador Válido",
            unidadeId,
            usuarioId
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ColaboradorResponse>();
        Assert.NotNull(body);
        Assert.Matches(@"^COL-\d{6}$", body!.Codigo);
        Assert.Equal("Colaborador Válido", body.Nome);
        Assert.Equal(unidadeId, body.Unidade.Id);
        Assert.Equal("Filial Colaborador", body.Unidade.Nome);
        Assert.Equal(usuarioId, body.UsuarioId);
    }

    [Fact]
    public async Task Create_ComUnidadeInativa_Retorna422()
    {
        var usuarioId = await CriarUsuarioAsync("colaborador.unidadeinativa");
        var unidadeInativaId = await CriarUnidadeInativaAsync("Filial Inativa");

        var response = await _client.PostAsJsonAsync("/api/colaboradores", new
        {
            nome = "Colaborador Unidade Inativa",
            unidadeId = unidadeInativaId,
            usuarioId
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Create_ComUsuarioJaVinculado_Retorna409()
    {
        var usuarioId = await CriarUsuarioAsync("colaborador.vinculado");
        var unidadeId = await CriarUnidadeAsync("Filial Vinculado");

        await _client.PostAsJsonAsync("/api/colaboradores", new
        {
            nome = "Primeiro Vínculo",
            unidadeId,
            usuarioId
        });

        var outraUnidadeId = await CriarUnidadeAsync("Filial Vinculado 2");

        var response = await _client.PostAsJsonAsync("/api/colaboradores", new
        {
            nome = "Segundo Vínculo",
            unidadeId = outraUnidadeId,
            usuarioId
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_ComUsuarioInexistente_Retorna404()
    {
        var unidadeId = await CriarUnidadeAsync("Filial Usuario 404");

        var response = await _client.PostAsJsonAsync("/api/colaboradores", new
        {
            nome = "Sem Usuario",
            unidadeId,
            usuarioId = 999999
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ComUnidadeInexistente_Retorna404()
    {
        var usuarioId = await CriarUsuarioAsync("colaborador.uni404");

        var response = await _client.PostAsJsonAsync("/api/colaboradores", new
        {
            nome = "Sem Unidade",
            unidadeId = 999999,
            usuarioId
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_AlterandoNomeEUnidade_Retorna200()
    {
        var usuarioId = await CriarUsuarioAsync("colaborador.update");
        var unidadeOrigemId = await CriarUnidadeAsync("Filial Origem");
        var unidadeDestinoId = await CriarUnidadeAsync("Filial Destino");

        var created = await _client.PostAsJsonAsync("/api/colaboradores", new
        {
            nome = "Nome Original",
            unidadeId = unidadeOrigemId,
            usuarioId
        });
        var criado = await created.Content.ReadFromJsonAsync<ColaboradorResponse>();

        var response = await _client.PutAsJsonAsync($"/api/colaboradores/{criado!.Id}", new
        {
            nome = "Nome Atualizado",
            unidadeId = unidadeDestinoId
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ColaboradorResponse>();
        Assert.NotNull(body);
        Assert.Equal("Nome Atualizado", body!.Nome);
        Assert.Equal(unidadeDestinoId, body.Unidade.Id);
        Assert.Equal("Filial Destino", body.Unidade.Nome);
    }

    [Fact]
    public async Task Update_ComUnidadeInativa_Retorna422()
    {
        var usuarioId = await CriarUsuarioAsync("colaborador.updateinativa");
        var unidadeAtivaId = await CriarUnidadeAsync("Filial Ativa Update");
        var unidadeInativaId = await CriarUnidadeInativaAsync("Filial Inativa Update");

        var created = await _client.PostAsJsonAsync("/api/colaboradores", new
        {
            nome = "Colaborador Update Inativa",
            unidadeId = unidadeAtivaId,
            usuarioId
        });
        var criado = await created.Content.ReadFromJsonAsync<ColaboradorResponse>();

        var response = await _client.PutAsJsonAsync($"/api/colaboradores/{criado!.Id}", new
        {
            unidadeId = unidadeInativaId
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Update_ComColaboradorInexistente_Retorna404()
    {
        var response = await _client.PutAsJsonAsync("/api/colaboradores/999999", new { nome = "Não Existe" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Retorna204EColaboradorNaoApareceMaisNaListagem()
    {
        var usuarioId = await CriarUsuarioAsync("colaborador.delete");
        var unidadeId = await CriarUnidadeAsync("Filial Delete");

        var created = await _client.PostAsJsonAsync("/api/colaboradores", new
        {
            nome = "Colaborador Delete",
            unidadeId,
            usuarioId
        });
        var criado = await created.Content.ReadFromJsonAsync<ColaboradorResponse>();

        var response = await _client.DeleteAsync($"/api/colaboradores/{criado!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var listagem = await _client.GetFromJsonAsync<List<ColaboradorListItemResponse>>("/api/colaboradores");
        Assert.DoesNotContain(listagem!, c => c.Id == criado.Id);
    }

    [Fact]
    public async Task Delete_ComColaboradorInexistente_Retorna404()
    {
        var response = await _client.DeleteAsync("/api/colaboradores/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task List_RetornaColaboradoresComUnidadeAssociada()
    {
        var usuarioId = await CriarUsuarioAsync("colaborador.list");
        var unidadeId = await CriarUnidadeAsync("Filial Listagem Colaborador");

        await _client.PostAsJsonAsync("/api/colaboradores", new
        {
            nome = "Colaborador Listagem",
            unidadeId,
            usuarioId
        });

        var response = await _client.GetAsync("/api/colaboradores");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<ColaboradorListItemResponse>>();
        Assert.NotNull(body);

        var colaborador = Assert.Single(body!, c => c.Nome == "Colaborador Listagem");
        Assert.Equal(unidadeId, colaborador.Unidade.Id);
        Assert.Equal("Filial Listagem Colaborador", colaborador.Unidade.Nome);
    }
}
