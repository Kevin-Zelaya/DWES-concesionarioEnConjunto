/// <summary>
/// Datos necesarios para actualizar una póliza.
/// </summary>
public class PolizaUpdateRequestDto
{
    public required string Nombre { get; set; }
    public required string Descripcion { get; set; }
    public decimal IncrementoDiario { get; set; }
}