using APIAlquilerVehiculos.Models.Reservas;
using APIAlquilerVehiculos.Models.Usuarios;
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
            

            entity.HasIndex(r => r.CodigoRefencia)
                .IsUnique();
            entity.Property(r => r.CodigoRefencia).HasValueGenerator<CodigoReferenciaGenerator>().ValueGeneratedOnAdd().HasMaxLength(6);
            // Relación con vehículo
            entity.HasOne(r => r.Vehiculo)
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

            // Categoría de vehículo solicitada
            entity.Property(r => r.Categoria)
                .IsRequired()
                .HasConversion<string>();

            // Fechas
            entity.Property(r => r.FechaCreacion)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");

            entity.Property(r => r.FechaRecogida)
                .IsRequired()
                .HasColumnType("timestamp with time zone");

            entity.Property(r => r.FechaDevolucion)
                .IsRequired()
                .HasColumnType("timestamp with time zone");

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
                .WithOne(l => l.Reserva)
                .HasForeignKey(l => l.ReservaId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            // control de concurrencia
            entity.Property(r => r.RowVersion)
                .IsRowVersion();
        });


        // ==========================================
        // LINEAS DE RESERVA
        // ==========================================
        modelbuilder.Entity<LineaReserva>(entity =>
        {
            entity.HasKey(l => l.Id);

            entity.Property(l => l.Id)
                .ValueGeneratedOnAdd();

            // Tipo: Póliza o Extra
            entity.Property(l => l.Tipo)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);
            
            entity.Property(l => l.TipoPrecio)
                .IsRequired()
                .HasConversion<string>();

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

            entity.Property(e => e.Precio)
                .IsRequired()
                .HasPrecision(10, 2);
            entity.Property(l => l.TipoPrecio)
                .IsRequired()
                .HasConversion<string>();

            entity.Property(e => e.Activa)
                .IsRequired()
                .HasDefaultValue(true);

            entity.HasQueryFilter(e => e.Activa);

            entity.HasIndex(e => e.Nombre);
        });

        modelbuilder.Entity<Poliza>(entity =>
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

            entity.Property(e => e.IncrementoDiario)
                .IsRequired()
                .HasPrecision(10, 2);

            entity.Property(e => e.Activa)
                .IsRequired()
                .HasDefaultValue(true);

            entity.HasQueryFilter(e => e.Activa);

            entity.HasIndex(e => e.Nombre);
        });


        // ==========================================
        // RECARGOS
        // ==========================================
        modelbuilder.Entity<Recargo>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.Property(r => r.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(r => r.Descripcion)
                .IsRequired()
                .HasMaxLength(250);

            entity.Property(r => r.TipoPrecio)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(r => r.Importe)
                .IsRequired()
                .HasPrecision(10, 2);

            entity.Property(r => r.Activa)
                .IsRequired()
                .HasDefaultValue(true);

            entity.HasQueryFilter(r => r.Activa);

            entity.HasIndex(r => r.Nombre);
        });
        // ==========================================
        // PAGOS
        // ==========================================
       
        modelbuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Id)
                .ValueGeneratedOnAdd();

            entity.HasOne(p => p.Reserva)
                .WithMany(r => r.Pagos)
                .HasForeignKey(p => p.ReservaId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(p => p.Importe)
                .IsRequired()
                .HasPrecision(10, 2);

            entity.Property(p => p.Divisa)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("EUR");

            entity.Property(p => p.Estado)
                .IsRequired()
                .HasConversion<string>();

            entity.Property(p => p.MarcaTarjeta)
                .IsRequired()
                .HasConversion<string>();

            entity.Property(p => p.Fecha)
                .IsRequired()
                .HasColumnType("timestamp with time zone");

            entity.Property(p => p.UltimosCuatroDigitos)
                .IsRequired()
                .HasMaxLength(4);

            entity.Property(p => p.TarjetaCifrada)
                .IsRequired();

            entity.Property(p => p.ReferenciaPasarela)
                .IsRequired()
                .HasMaxLength(150);

            entity.HasIndex(p => p.ReservaId);
            entity.HasIndex(p => p.ReferenciaPasarela)
                .IsUnique();
        });

    }
}