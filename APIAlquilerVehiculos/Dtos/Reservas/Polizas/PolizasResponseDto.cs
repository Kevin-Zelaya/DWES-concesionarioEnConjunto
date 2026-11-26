/// <summary>
/// Información de una póliza.
/// </summary>
public class PolizaResponseDto
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal IncrementoDiario { get; set; }
    public bool Activa { get; set; }
}