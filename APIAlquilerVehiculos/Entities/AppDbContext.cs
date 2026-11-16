using APIAlquilerVehiculos.Entities.FluenApi;
using APIAlquilerVehiculos.Models;
using Microsoft.EntityFrameworkCore;

namespace APIAlquilerVehiculos.Entities;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    
	public DbSet<Vehiculo> Vehiculos {get; set;}
	public DbSet<Modelo> Modelos {get; set;}
	// separar en otro archivo el fluent api
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		// COnfiguración de vehículo y sus relacionados
		VehiculoYRelacionadosConfig.EntityConfigure(modelBuilder);
	}
}