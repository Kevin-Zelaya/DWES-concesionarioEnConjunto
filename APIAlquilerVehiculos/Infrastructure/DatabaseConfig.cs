using APIAlquilerVehiculos.Entities;
using Microsoft.EntityFrameworkCore;

public static class DatabaseConfig
{
    public static void AddDatabases(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        //Log.Information("Configurando la base de datos...");
        var dbConfig = configuration
            .GetSection("DbConnections")
            .Get<DbConnectionsConfig>();
        services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(dbConfig!.Postgre.ConnectionStrings.DefaultConnection));
    }
}