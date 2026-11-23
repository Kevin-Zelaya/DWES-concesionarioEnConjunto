using APIAlquilerVehiculos.Models;
using Microsoft.EntityFrameworkCore;

namespace APIAlquilerVehiculos.Entities.FluentApi;

public static class ParteConfig
{
    public static void EntityConfigure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Parte>(entity =>
        {
            entity.ToTable("partes");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Descripcion)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(p => p.Estado)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.HasOne(p => p.Vehiculo)
                .WithMany()
                .HasForeignKey(p => p.VehiculoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.QuienLoTramita)
                .WithMany()
                .HasForeignKey(p => p.QuienLoTramitaId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}