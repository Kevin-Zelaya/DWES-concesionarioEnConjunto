/// <summary>
/// Datos necesarios para crear un extra.
/// </summary>
public class ExtraCreateRequestDto
{
    public required string Nombre { get; set; }
    public required string Descripcion { get; set; }
    public decimal Precio { get; set; }
    public TipoPrecio TipoPrecio { get; set; }
}