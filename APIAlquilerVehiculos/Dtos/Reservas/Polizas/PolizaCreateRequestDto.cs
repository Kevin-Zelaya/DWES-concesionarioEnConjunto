/// <summary>
/// Datos necesarios para crear una póliza.
/// </summary>
public class PolizaCreateRequestDto
{
    public required string Nombre { get; set; }
    public required string Descripcion { get; set; }
    public decimal IncrementoDiario { get; set; }
}