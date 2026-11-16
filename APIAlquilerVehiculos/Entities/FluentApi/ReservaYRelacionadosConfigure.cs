using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Models.Enums;
using APIAlquilerVehiculos.Models.Reservas;
using Microsoft.EntityFrameworkCore;

public class ReservaYRelacionadosConfigure
{
    public static void EntityConfigure(ModelBuilder modelbuilder)
    {
        // ==========================================
        // RESERVAS
        // ==========================================
        modelbuilder.Entity<Reserva>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.Property(r => r.Id)
                .ValueGeneratedOnAdd();

            // Código de referencia único
            entity.Property(r => r.CodigoRefencia)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(r => r.CodigoRefencia)
                .IsUnique();

            // Relación con vehículo
            entity.HasOne<Vehiculo>()
                .WithMany()
                .HasForeignKey(r => r.VehiculoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación con cliente
            entity.HasOne<Cliente>()
                .WithMany()
                .HasForeignKey(r => r.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación con trabajador
            entity.HasOne<Trabajador>()
                .WithMany()
                .HasForeignKey(r => r.TrabajadorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación con pago
            entity.HasOne<Pago>()
                .WithOne()
                .HasForeignKey<Reserva>(r => r.PagoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Categoría de vehículo solicitada
            entity.Property(r => r.Categoria)
                .IsRequired()
                .HasConversion<string>();

            // Fechas
            entity.Property(r => r.FechaCreacion)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(r => r.FechaRecogida)
                .IsRequired();

            entity.Property(r => r.FechaDevolucion)
                .IsRequired();

            // Estado de la reserva
            entity.Property(r => r.Estado)
                .IsRequired()
                .HasConversion<string>()
                .HasDefaultValue(Estado.Pendiente);

            // Precio total
            entity.Property(r => r.PrecioTotal)
                .IsRequired()
                .HasPrecision(10, 2);

            // Notas opcionales
            entity.Property(r => r.Notas)
                .IsRequired(false)
                .HasMaxLength(500);

            // Relación con líneas de reserva
            entity.HasMany(r => r.Lineas)
                .WithOne()
                .HasForeignKey(l => l.ReservaId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        // ==========================================
        // LINEAS DE RESERVA
        // ==========================================
        modelbuilder.Entity<LineaReserva>(entity =>
        {
            entity.HasKey(l => l.Id);

            entity.Property(l => l.Id)
                .ValueGeneratedOnAdd();

            // Relación con Reserva
            entity.Property(l => l.ReservaId)
                .IsRequired();

            // Tipo: Póliza o Extra
            entity.Property(l => l.Tipo)
                .IsRequired()
                .HasMaxLength(50);

            // Concepto descriptivo
            entity.Property(l => l.Concepto)
                .IsRequired()
                .HasMaxLength(250);

            entity.Property(l => l.Cantidad)
                .IsRequired();

            // Importes económicos
            entity.Property(l => l.PrecioUnitario)
                .IsRequired()
                .HasPrecision(10, 2);

            entity.Property(l => l.Subtotal)
                .IsRequired()
                .HasPrecision(10, 2);

            // Índice para consultar las líneas de una reserva
            entity.HasIndex(l => l.ReservaId);
        });


        // ==========================================
        // EXTRAS
        // ==========================================
        modelbuilder.Entity<Extra>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Descripcion)
                .IsRequired()
                .HasMaxLength(250);

            entity.Property(e => e.PrecioDiario)
                .IsRequired()
                .HasPrecision(10, 2);

            entity.Property(e => e.Activa)
                .IsRequired()
                .HasDefaultValue(true);

            entity.HasQueryFilter(e => e.Activa);

            entity.HasIndex(e => e.Nombre);
        });


        // ==========================================
        // PAGOS
        // ==========================================
        modelbuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Id)
                .ValueGeneratedOnAdd();

            entity.Property(p => p.Importe)
                .IsRequired();

            // Divisa por defecto EUR
            entity.Property(p => p.Divisa)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("EUR");

            // Estado del pago
            entity.Property(p => p.Estado)
                .IsRequired()
                .HasConversion<string>();

            // Marca de la tarjeta
            entity.Property(p => p.MarcaTarjeta)
                .IsRequired()
                .HasConversion<string>();

            // Últimos dígitos de la tarjeta
            entity.Property(p => p.UltimosDigito)
                .IsRequired(false)
                .HasMaxLength(4);

            // Número de tarjeta cifrado
            entity.Property(p => p.TarjetaCifrada)
                .IsRequired(false);
        });
    }
}