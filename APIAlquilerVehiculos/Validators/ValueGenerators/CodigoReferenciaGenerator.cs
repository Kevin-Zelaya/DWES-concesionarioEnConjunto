using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;

public class CodigoReferenciaGenerator : ValueGenerator<string>
{
    private const string Caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public override bool GeneratesTemporaryValues => false;

    public override string Next(EntityEntry entry)
    {
        return GenerarCodigo(6);
    }

    public static string GenerarCodigo(int longitud = 6)
    {
        return new string(
            Enumerable.Range(0, longitud)
                .Select(_ => Caracteres[RandomNumberGenerator.GetInt32(Caracteres.Length)])
                .ToArray()
        );
    }
}