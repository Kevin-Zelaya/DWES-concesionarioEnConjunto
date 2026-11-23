using APIAlquilerVehiculos.Errors.Common;

/// <summary>
/// Proporciona un catálogo centralizado de errores de dominio específicos para las operaciones de almacenamiento y subida de archivos.
/// </summary>
public static class StorageError
{
    /// <summary>
    /// Crea un error indicando que no se ha adjuntado ningún archivo o que este viene vacío.
    /// </summary>
    public static DomainError ArchivoVacio() =>
        new BusinessRuleError("El archivo esta vacio o no se ha proporcionado");

    /// <summary>
    /// Crea un error indicando que el archivo excede el límite de tamaño permitido (configurado en 5 MB).
    /// </summary>
    public static DomainError ArchivoMuyGrande() =>
        new BusinessRuleError("El archivo supera el tamano maximo permitido (5 MB)");

    /// <summary>
    /// Crea un error indicando que la extensión del archivo no está entre las permitidas (.jpg, .jpeg, .png, .gif).
    /// </summary>
    public static DomainError ExtensionNoPermitida() =>
        new BusinessRuleError("La extension del archivo no esta permitida. Use: .jpg, .jpeg, .png, .gif");

    /// <summary>
    /// Crea un error indicando que el Content-Type no corresponde a los formatos de imagen permitidos.
    /// </summary>
    public static DomainError TipoContenidoNoPermitido() =>
        new BusinessRuleError("El tipo de contenido no esta permitido. Solo se permiten imagenes");

    /// <summary>
    /// Crea un error indicando que el nombre del archivo contiene caracteres prohibidos o maliciosos.
    /// </summary>
    public static DomainError NombreArchivoInvalido() =>
        new BusinessRuleError("El nombre del archivo contiene caracteres no validos");

    /// <summary>
    /// Crea un error interno para cuando falla el almacenamiento físico o en la nube del archivo.
    /// </summary>
    public static DomainError ErrorGuardando() =>
        new InternalError("Error al guardar el archivo en el servidor");

    /// <summary>
    /// Crea un error interno para cuando falla la eliminación física o en la nube del archivo.
    /// </summary>
    public static DomainError ErrorEliminando() =>
        new InternalError("Error al eliminar el archivo del servidor");
}