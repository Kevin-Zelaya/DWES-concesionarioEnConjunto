
namespace APIAlquilerVehiculos.Models.Cassandra.Enums;

/// <summary>
/// Tipos de eventos que pueden registrarse en el sistema.
/// </summary>
public enum TipoEvento
{
    ReservaCreada,
    ReservaConfirmada,
    PagoAprobado,
    Recogida,
    Devolucion,
    Cancelacion,
    Caducidad,
    Reembolso,
    NoPresentado,
    EntradaTaller,
    SalidaTaller
}
