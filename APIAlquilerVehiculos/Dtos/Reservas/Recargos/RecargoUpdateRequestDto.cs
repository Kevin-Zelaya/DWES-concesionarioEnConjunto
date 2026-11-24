/// <summary>
/// Datos necesarios para actualizar un recargo.
/// </summary>
public class RecargoUpdateRequestDto
{
    public required string Nombre { get; set; }
    public required string Descripcion { get; set; }
    public TipoPrecio TipoPrecio { get; set; }
    public decimal Importe { get; set; }
}