using GestaoColaboradores.API.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoColaboradores.API.Infra.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.Property(u => u.Codigo).IsRequired();
        builder.Property(u => u.Login).IsRequired();
        builder.Property(u => u.Senha).IsRequired();

        builder.Property(u => u.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.HasIndex(u => u.Codigo).IsUnique();
        builder.HasIndex(u => u.Login).IsUnique();
    }
}
