using System.Net;
using APIAlquilerVehiculos.Errors.Common;

namespace APIAlquilerVehiculos.Errors.Vehiculo;

/// <summary>
/// Errores de dominio especificos para Vehiculos
/// </summary>
/// <param name="mensaje">Mensaje de error</param>
/// <param name="StatusCode">Codigo de error Http</param>
public sealed record VehiculoErrors(string mensaje, HttpStatusCode StatusCode): DomainError(mensaje,StatusCode)
{
    /// <summary>
    /// Error de matrícula inválida según los criterios de matriculates de españa
    /// </summary>
    /// <param name="matricula"> matricula invalida</param>
    /// <returns>devuelve el error</returns>
    public static VehiculoErrors InvalidMatricula(string matricula) =>
        new($"La matricula {matricula} tiene un formato invalido", HttpStatusCode.Conflict);

    /// <summary>
    /// Error de codigo acris el código real del sector inválido 
    /// </summary>
    /// <param name="acriss"> codigo acriss </param>
    /// <returns>devuelve el error</returns>
    public static VehiculoErrors InvalidAcriss(string acriss) =>
        new($"El codigo acris {acriss} es invalido", HttpStatusCode.BadRequest);
    
    /// <summary>
    /// Error de categoria por una categoria fuera de las acceptadas en las reglas de negocio 
    /// </summary>
    /// <param name="categoria"> categoria invalida </param>
    /// <returns>devuelve el error</returns>
    public static VehiculoErrors InvalidCategoria (string categoria) =>
    new ($"La categoria {categoria} es invalida", HttpStatusCode.BadRequest);

    /// <summary>
    /// Error de categoria por una categoria que no coincide con el codigo acris según las reglas de negocio 
    /// </summary>
    /// <param name="categoria"> categoria invalida </param>
    /// <param name="acriss"> codigo acriss </param>
    /// <returns>devuelve el error</returns>
    public static VehiculoErrors ErrorDeRelacion(string categoria, string acris) =>
        new($"La categoria {categoria} no se relaciona con el acris {acris}", HttpStatusCode.UnprocessableEntity);

    /// <summary>
    /// Error de combustible por un tipo fuera de rango
    /// </summary>
    /// <param name="combustible"> combustible usado</param>
    /// <returns>devuelve el error</returns>
    public static VehiculoErrors InvalidCombustible(string combustible) =>
        new($"El combustible {combustible} es invalido", HttpStatusCode.BadRequest);
}