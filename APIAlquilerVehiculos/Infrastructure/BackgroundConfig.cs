using Serilog;

public static class BackgroundConfig
{
   
    public static IServiceCollection AddBackground(
        this IServiceCollection services
    )
    {
        Log.Information("Configurando servicios en segundo plano...");
        services.AddHostedService<ReservaBackgroundService>();
        return services;
    }
}