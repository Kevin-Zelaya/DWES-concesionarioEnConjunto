using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Models.Enums;
using APIAlquilerVehiculos.Validators.ValueGenerators;
using Microsoft.EntityFrameworkCore;

namespace APIAlquilerVehiculos.Entities.FluenApi;

public static class VehiculoYRelacionadosConfig
{
    public static void EntityConfigure(ModelBuilder modelBuilder)
    {
        // ==========================================
        // VEHICULOS
        // ==========================================
        modelBuilder.Entity<Vehiculo>(entity =>
        {
            entity.HasKey(v => v.Id);

            // Identificador generado por el servidor
            entity.Property(v => v.Id).HasValueGenerator<YoutubeIdValueGenerator>().ValueGeneratedOnAdd().HasMaxLength(11);

            // Matrícula única
            entity.HasIndex(v => v.Matricula).IsUnique();

            entity.Property(v => v.Matricula).IsRequired().HasMaxLength(7);

            // Código ACRISS
            entity.Property(v => v.Acriss).IsRequired().HasMaxLength(4);

            // Categoría de negocio
            entity.Property(v => v.categoria).IsRequired().HasConversion<string>();

            // Características
            entity.Property(v => v.Color).IsRequired();

            entity.Property(v => v.Combustible).IsRequired();

            entity.Property(v => v.Maletero).IsRequired();

            entity.Property(v => v.NumeroPlazas).IsRequired();

            entity.Property(v => v.Autonomia).IsRequired();

            entity.Property(v => v.Imagenes).IsRequired();

            // Estado del vehículo
            entity.Property(v => v.Estado).IsRequired().HasConversion<string>().HasDefaultValue(EstadoEnum.Disponible);

            // Fecha de revisión
            entity.Property(v => v.UltimaRevision).IsRequired();

            // Relaciones
            entity.HasOne<Marca>()
                .WithMany()
                .HasForeignKey(v => v.Marca_id).OnDelete(DeleteBehavior.Restrict); // no se elimina en cascada

            entity.HasOne<Modelo>()
                .WithMany()
                .HasForeignKey(v => v.Modelo_id)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ==========================================
        // MODELOS
        // ==========================================
        modelBuilder.Entity<Modelo>(entity =>
        {
            entity.HasKey(m => m.Id);

            entity.Property(m => m.Id).ValueGeneratedOnAdd();

            entity.Property(m => m.Nombre).IsRequired();

            entity.Property(m => m.Descricion).IsRequired();

            entity.Property(m => m.Acriss).IsRequired().HasMaxLength(4);

            // Índice para búsquedas por nombre
            entity.HasIndex(m => m.Nombre);
        });

        // ==========================================
        // MARCAS
        // ==========================================
        modelBuilder.Entity<Marca>(entity =>
        {
            entity.HasKey(m => m.Id);

            entity.Property(m => m.Id).ValueGeneratedOnAdd();

            entity.Property(m => m.Nombre).IsRequired();

            entity.Property(m => m.Wb).IsRequired();

            entity.Property(m => m.Activo).IsRequired()
                .HasDefaultValue(true);

            // Índice para búsquedas por nombre
            entity.HasIndex(m => m.Nombre);

            // Filtro de baja lógica
            entity.HasQueryFilter(m => m.Activo);
        });
    }
}