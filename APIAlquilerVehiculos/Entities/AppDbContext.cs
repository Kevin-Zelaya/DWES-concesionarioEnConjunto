using APIAlquilerVehiculos.Entities.FluenApi;
using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Models.Reservas;
using APIAlquilerVehiculos.Models.Usuarios;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace APIAlquilerVehiculos.Entities;

public class AppDbContext(DbContextOptions<AppDbContext> options)
	: IdentityDbContext<Usuario, IdentityRole<long>, long>(options)
{
    
	public DbSet<Vehiculo> Vehiculos {get; set;}
	public DbSet<Modelo> Modelos {get; set;}
	public DbSet<Marca> Marcas {get; set;}

	// Reservas
	public DbSet<Reserva> Reservas {get; set;}
	public DbSet<LineaReserva> Lineas {get; set;}
	public DbSet<Poliza> Polizas {get; set;}
	public DbSet<Extra> Extras {get; set;}
	// separar en otro archivo el fluent api
	
	//USUARIOS
	public DbSet<Cliente> Cliente {get; set;}
	public DbSet<Trabajador> Trabajador {get; set;}
	public DbSet<Oficina> Oficinas { get; set; }
	
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		
		modelBuilder.HasPostgresExtension("btree_gist");
		// COnfiguración de vehículo y sus relacionados
		VehiculoYRelacionadosConfig.EntityConfigure(modelBuilder);
		ReservaYRelacionadosConfigure.EntityConfigure(modelBuilder);
		UsuarioYRelacionadosConfig.EntityConfigure(modelBuilder);

		//TABLAS DE IDENTITY POR DEFECTO
		modelBuilder.Entity<IdentityUserClaim<long>>().ToTable("user_claims");
		modelBuilder.Entity<IdentityUserRole<long>>().ToTable("user_roles");
		modelBuilder.Entity<IdentityUserLogin<long>>().ToTable("user_logins");
		modelBuilder.Entity<IdentityUserToken<long>>().ToTable("user_tokens");
		modelBuilder.Entity<IdentityRoleClaim<long>>().ToTable("role_claims");
	}
}