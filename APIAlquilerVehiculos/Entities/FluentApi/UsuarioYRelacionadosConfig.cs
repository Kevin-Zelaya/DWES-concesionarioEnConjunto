using APIAlquilerVehiculos.Models;
using Microsoft.EntityFrameworkCore;

public static class UsuarioYRelacionadosConfig
{
    public static void EntityConfigure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.HasOne(c => c.Usuario)
                .WithOne(u => u.Cliente)
                .HasForeignKey<Cliente>(c => c.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(c => c.IdUsuario)
                .IsUnique();
        });

        modelBuilder.Entity<Trabajador>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.HasOne(t => t.Usuario)
                .WithOne(u => u.Trabajador)
                .HasForeignKey<Trabajador>(t => t.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(t => t.IdUsuario)
                .IsUnique();
        });
    }
}