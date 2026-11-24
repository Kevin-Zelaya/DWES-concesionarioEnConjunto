/// <summary>
/// Datos necesarios para crear un recargo.
/// </summary>
public class RecargoCreateRequestDto
{
    public required string Nombre { get; set; }
    public required string Descripcion { get; set; }
    public TipoPrecio TipoPrecio { get; set; }
    public decimal Importe { get; set; }
}