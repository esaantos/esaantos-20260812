using GestaoColaboradores.API.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoColaboradores.API.Infra.Configurations;

public class UnidadeConfiguration : IEntityTypeConfiguration<Unidade>
{
    public void Configure(EntityTypeBuilder<Unidade> builder)
    {
        builder.Property(u => u.CodigoUnidade).IsRequired();
        builder.Property(u => u.Nome).IsRequired();

        builder.Property(u => u.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.HasIndex(u => u.CodigoUnidade).IsUnique();
    }
}
