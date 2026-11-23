namespace APIAlquilerVehiculos.Services.Pagos;

public interface ICifradorTarjeta
{
    string Cifrar(string numeroTarjeta);
}