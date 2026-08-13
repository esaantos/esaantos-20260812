using GestaoColaboradores.API.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoColaboradores.API.Infra.Configurations;

public class ColaboradorConfiguration : IEntityTypeConfiguration<Colaborador>
{
    public void Configure(EntityTypeBuilder<Colaborador> builder)
    {
        builder.Property(c => c.Codigo).IsRequired();
        builder.Property(c => c.Nome).IsRequired();

        builder.HasIndex(c => c.Codigo).IsUnique();

        // 1:1 — um Usuario nunca pode estar vinculado a mais de um Colaborador.
        builder.HasIndex(c => c.UsuarioId).IsUnique();

        builder.HasOne(c => c.Unidade)
            .WithMany(u => u.Colaboradores)
            .HasForeignKey(c => c.UnidadeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Usuario)
            .WithOne(u => u.Colaborador)
            .HasForeignKey<Colaborador>(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
