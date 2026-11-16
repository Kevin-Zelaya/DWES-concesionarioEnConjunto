
using APIAlquilerVehiculos.Entities;

public static class InitializationDb
{
    // Inicialización normal: conservar datos
    public static async Task InitializeDatabasesAsync(
        this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        // PostgreSQL
        await context.Database.EnsureCreatedAsync();


        //Log.Information("[DATABASES] Bases de datos inicializadas.");
    }

    // Reconstrucción: eliminar datos
    public static async Task RecreateDatabasesAsync(
        this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        // var redis = scope.ServiceProvider
        //     .GetRequiredService<IConnectionMultiplexer>();

        // var server = redis.GetServer(redis.GetEndPoints().First());

        // PostgreSQL
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        //Log.Information("[POSTGRES] Base de datos recreada.");


        // // Redis
        // await server.FlushAllDatabasesAsync();
    }
}