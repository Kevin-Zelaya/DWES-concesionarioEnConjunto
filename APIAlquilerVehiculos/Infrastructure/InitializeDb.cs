
using APIAlquilerVehiculos.Entities;
using Microsoft.EntityFrameworkCore;

public static class InitializationDb
{
    // Inicialización normal: conservar datos y aplicar migraciones pendientes.
    public static async Task InitializeDatabasesAsync(
        this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        // PostgreSQL
        await context.Database.MigrateAsync();

        //Log.Information("[DATABASES] Bases de datos inicializadas.");
    }

    // Reconstrucción: eliminar datos y aplicar todas las migraciones.
    public static async Task RecreateDatabasesAsync(
        this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        // PostgreSQL
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();

        //Log.Information("[POSTGRES] Base de datos recreada.");

        // Redis
        // var redis = scope.ServiceProvider
        //     .GetRequiredService<IConnectionMultiplexer>();

        // var server = redis.GetServer(redis.GetEndPoints().First());
        // await server.FlushAllDatabasesAsync();
    }
}
