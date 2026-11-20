using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Models.Usuarios;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public static class UsuarioYRelacionadosConfig
{
    public static void EntityConfigure(ModelBuilder modelBuilder)
    {
        // users
        modelBuilder.Entity<Usuario>(entity =>
        {   
            entity.ToTable("usuarios");
            entity.HasKey(c => c.Id);

            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.HasIndex(u => u.Email).IsUnique();

            entity.Property(u => u.CreatedAt).IsRequired();
        });
        
        // roles
        modelBuilder.Entity<IdentityRole<long>>(entity => {
            entity.ToTable("roles");
        });
        
        // clientes
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.HasOne(c => c.Usuario)
                .WithOne(u => u.Cliente)
                .HasForeignKey<Cliente>(c => c.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(c => c.IdUsuario).IsUnique();
            
            entity.HasIndex(c => c.Dni).IsUnique();
            entity.HasIndex(c => c.Nie).IsUnique();
            entity.HasIndex(c => c.Pasaporte).IsUnique();
            
            entity.Property(u => u.IsActivo).IsRequired();
            entity.HasQueryFilter(p => p.IsActivo);
        });

        // trabajadores
        modelBuilder.Entity<Trabajador>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.HasOne(t => t.Usuario)
                .WithOne(u => u.Trabajador)
                .HasForeignKey<Trabajador>(t => t.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(t => t.IdUsuario).IsUnique();
            entity.HasIndex(t => t.Dni).IsUnique();
            
            entity.Property(u => u.IsActivo).IsRequired();
            entity.HasQueryFilter(p => p.IsActivo);
        });
        
        // oficina
        modelBuilder.Entity<Oficina>(entity =>
        {
            entity.HasKey(o => o.Id);

            entity.Property(o => o.Name).HasMaxLength(150).IsRequired();
            entity.Property(o => o.Telefono).HasMaxLength(20).IsRequired();
            entity.Property(o => o.Horario).HasMaxLength(100).IsRequired();

            entity.OwnsOne(o => o.Direccion, dir =>
            {
                dir.Property(d => d.Calle).HasColumnName("calle").HasMaxLength(200).IsRequired();
                dir.Property(d => d.CodigoPostal).HasColumnName("codigo_postal").HasMaxLength(10).IsRequired();
                dir.Property(d => d.Ciudad).HasColumnName("ciudad").HasMaxLength(100).IsRequired();
            });

            entity.OwnsOne(o => o.Localizacion, loc =>
            {
                loc.Property(l => l.Latitud).HasColumnName("latitud").HasPrecision(9, 6);
                loc.Property(l => l.Longitud).HasColumnName("longitud").HasPrecision(9, 6);
            });

            entity.HasOne(o => o.Responsable)
                .WithMany()
                .HasForeignKey(o => o.IdResponsable)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}