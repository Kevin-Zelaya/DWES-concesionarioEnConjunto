/// <summary>
/// Datos necesarios para actualizar un extra.
/// </summary>
public class ExtraUpdateRequestDto
{
    public required string Nombre { get; set; }
    public required string Descripcion { get; set; }
    public decimal Precio { get; set; }
    public TipoPrecio TipoPrecio { get; set; }
    public bool Activa { get; set; }
}