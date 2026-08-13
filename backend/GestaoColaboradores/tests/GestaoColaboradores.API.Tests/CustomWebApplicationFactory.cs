using System.Collections.Concurrent;
using GestaoColaboradores.API.Infra;
using GestaoColaboradores.API.Services.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GestaoColaboradores.API.Tests;

// SqlQueryRaw + nextval() em CodigoSequencialGenerator exige provider relacional (Npgsql).
// O host de teste usa InMemory, então trocamos por um gerador fake em memória que segue
// o mesmo formato (PREFIXO-NNNNNN), sem depender de banco.
public class FakeCodigoSequencialGenerator : ICodigoSequencialGenerator
{
    private readonly ConcurrentDictionary<string, long> _contadores = new();

    public Task<string> ProximoAsync(string sequenceName, string prefixo, CancellationToken ct = default)
    {
        var proximo = _contadores.AddOrUpdate(sequenceName, 1, (_, v) => v + 1);
        return Task.FromResult(CodigoSequencialGenerator.Formatar(proximo, prefixo));
    }
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"TestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-only-jwt-signing-key-nao-e-um-segredo-real-0123456789",
                ["Jwt:Issuer"] = "GestaoColaboradores.Tests",
                ["Jwt:Audience"] = "GestaoColaboradores.Tests",
                ["Jwt:ExpiresInMinutes"] = "60"
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            // Provider interno dedicado ao InMemory: evita conflito com os serviços do
            // provider Npgsql que o Program.cs já registrou no container principal.
            var inMemoryServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
                options.UseInternalServiceProvider(inMemoryServiceProvider);
            });

            services.RemoveAll<ICodigoSequencialGenerator>();
            services.AddSingleton<ICodigoSequencialGenerator, FakeCodigoSequencialGenerator>();
        });
    }

    public async Task SeedAsync(Func<AppDbContext, Task> seed)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await seed(db);
        await db.SaveChangesAsync();
    }
}
