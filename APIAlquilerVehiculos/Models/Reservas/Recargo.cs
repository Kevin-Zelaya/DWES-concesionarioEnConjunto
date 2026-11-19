public class Recargo
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public TipoPrecio TipoPrecio { get; set; }
    public decimal Importe { get; set; }
    public bool Activa { get; set; }
}

