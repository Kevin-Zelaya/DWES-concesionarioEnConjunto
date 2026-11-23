using System.Security.Cryptography;
using System.Text;

namespace APIAlquilerVehiculos.Services.Pagos;

public class CifradorTarjeta(IConfiguration configuration) : ICifradorTarjeta
{
    private readonly byte[] _clave = Convert.FromBase64String(
        configuration["Cifrado:Clave"]
        ?? throw new InvalidOperationException("No se ha configurado la clave de cifrado.")
    );

    public string Cifrar(string numeroTarjeta)
    {
        if (_clave.Length != 32)
            throw new InvalidOperationException("La clave debe tener 32 bytes.");

        byte[] nonce = RandomNumberGenerator.GetBytes(12);
        byte[] textoPlano = Encoding.UTF8.GetBytes(numeroTarjeta);

        byte[] textoCifrado = new byte[textoPlano.Length];
        byte[] tag = new byte[16];

        using var aes = new AesGcm(_clave, 16);

        aes.Encrypt(nonce, textoPlano, textoCifrado, tag);

        // Guardamos nonce + tag + texto cifrado.
        byte[] resultado = nonce.Concat(tag).Concat(textoCifrado).ToArray();

        return Convert.ToBase64String(resultado);
    }
}