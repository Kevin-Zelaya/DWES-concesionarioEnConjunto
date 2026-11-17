using Serilog;

/// <summary>
/// Proporciona métodos de extensión para registrar y configurar los servicios de almacenamiento de archivos en el contenedor de dependencias.
/// </summary>
public static class StorageConfig
{
    /// <summary>
    /// Configura e inyecta los servicios de almacenamiento de archivos en la colección de servicios de la aplicación.
    /// </summary>
    /// <param name="services">Colección de servicios de la aplicación.</param>
    /// <returns>La misma colección de servicios para permitir llamadas encadenadas (*fluent interface*).</returns>
    public static IServiceCollection AddStorage(
        this IServiceCollection services
    )
    {
        Log.Information("Configurando el almacenamiento de archivos...");
        
        // Registra el servicio de almacenamiento físico en disco como un servicio único (Singleton)
        services.AddSingleton<IStorageService, FileSystemStorageService>();
        
        Log.Information("Almacenamiento de archivos configurado correctamente.");
        return services;
    }
}