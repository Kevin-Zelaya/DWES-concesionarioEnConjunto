using Serilog;

/// <summary>
/// Proporciona métodos de extensión para inicializar y preparar el directorio de almacenamiento local al arrancar la aplicación.
/// </summary>
public static class StorageInitializationExtensions
{
    /// <summary>
    /// Verifica, limpia o crea la estructura de carpetas de almacenamiento ("uploads") en función del entorno de ejecución.
    /// </summary>
    /// <param name="app">Instancia de la aplicación web.</param>
    /// <param name="isDevelopment">Indica si la aplicación se está ejecutando en un entorno de desarrollo.</param>
    public static void InitializeStorage(this WebApplication app, bool isDevelopment)
    {
        var rootPath = app.Environment.WebRootPath ?? app.Environment.ContentRootPath;
        var storagePath = System.IO.Path.Combine(rootPath, "uploads");
        var storageDirectory = new DirectoryInfo(storagePath);

        if (isDevelopment)
        {
            Log.Information("[DESARROLLO] Preparando directorio de almacenamiento: {Path}", storagePath);

            try
            {
                if (storageDirectory.Exists)
                {
                    // Limpia los ficheros previos preservando el recurso por defecto (ej. placeholder de imagen)
                    foreach (var file in storageDirectory.GetFiles("*", SearchOption.AllDirectories))
                    {
                        if (file.Name != "default.png")
                            file.Delete();
                    }

                    Log.Information("Contenido del directorio borrado");
                }

                if (!storageDirectory.Exists)
                    storageDirectory.Create();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al preparar directorio de almacenamiento");
            }
        }
        else
        {
            Log.Information("[PRODUCCION] Verificando directorio de almacenamiento: {Path}", storagePath);

            try
            {
                // En producción solo se asegura de que exista la carpeta sin destruir datos previos
                if (!storageDirectory.Exists)
                    storageDirectory.Create();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al verificar directorio de almacenamiento");
            }
        }
    }
}