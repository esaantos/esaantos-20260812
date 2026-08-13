using GestaoColaboradores.API.Domain;
using Microsoft.EntityFrameworkCore;

namespace GestaoColaboradores.API.Infra;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Colaborador> Colaboradores => Set<Colaborador>();
    public DbSet<Unidade> Unidades => Set<Unidade>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.HasSequence<long>("usuario_codigo_seq").StartsAt(1).IncrementsBy(1);
        modelBuilder.HasSequence<long>("colaborador_codigo_seq").StartsAt(1).IncrementsBy(1);
        modelBuilder.HasSequence<long>("unidade_codigo_seq").StartsAt(1).IncrementsBy(1);

        base.OnModelCreating(modelBuilder);
    }
}
