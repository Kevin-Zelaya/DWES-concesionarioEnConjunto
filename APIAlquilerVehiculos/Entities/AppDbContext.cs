using APIAlquilerVehiculos.Entities.FluenApi;
using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Models.Reservas;
using Microsoft.EntityFrameworkCore;

namespace APIAlquilerVehiculos.Entities;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    
	public DbSet<Vehiculo> Vehiculos {get; set;}
	public DbSet<Modelo> Modelos {get; set;}

	// Reservas
	public DbSet<Reserva> Reservas {get; set;}
	public DbSet<LineaReserva> Lineas {get; set;}
	public DbSet<Poliza> Polizas {get; set;}
	public DbSet<Extra> Extras {get; set;}
	// separar en otro archivo el fluent api
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasPostgresExtension("btree_gist");
		// COnfiguración de vehículo y sus relacionados
		VehiculoYRelacionadosConfig.EntityConfigure(modelBuilder);
		ReservaYRelacionadosConfigure.EntityConfigure(modelBuilder);
		UsuarioYRelacionadosConfig.EntityConfigure(modelBuilder);
	}
}