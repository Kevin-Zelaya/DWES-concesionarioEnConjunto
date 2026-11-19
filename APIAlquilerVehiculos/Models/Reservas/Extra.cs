/// <summary>
/// Extras, entre ellos precio por conductor joven
/// </summary>
public class Extra
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public TipoPrecio TipoPrecio {get; set;}
    public bool Activa { get; set; }
}