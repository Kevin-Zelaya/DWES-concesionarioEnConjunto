using System.Net;
using System.Text;
using APIAlquilerVehiculos.Dtos.Email;

namespace APIAlquilerVehiculos.Services.Email;

/// <summary>
/// Plantillas HTML comunes para los correos de Alquiler de Vehículos.
/// </summary>
public static class EmailTemplates
{
    public static string CreateBase(string title, string content)
    {
        return $@"<!DOCTYPE html>
<html lang='es'>
<head><meta charset='UTF-8'><title>{Encode(title)}</title></head>
<body style='font-family: Arial, sans-serif; background: #f0f2f5; padding: 20px;'>
<div style='max-width: 600px; margin: 0 auto; background: #fff; border-radius: 12px; overflow: hidden;'>
    <div style='background: linear-gradient(135deg, #667eea, #764ba2); padding: 30px; text-align: center;'>
        <h1 style='color: #fff; margin: 0;'>Alquiler de Vehículos - IES Luis Vives</h1>
    </div>
    <div style='padding: 30px;'>
        <h2 style='color: #1a1a2e; border-bottom: 2px solid #667eea; padding-bottom: 10px;'>{Encode(title)}</h2>
        <div style='color: #4a4a68; line-height: 1.8;'>{content}</div>
    </div>
    <div style='background: #f8f9fa; padding: 20px; text-align: center; border-top: 1px solid #e9ecef;'>
        <p style='margin: 0; color: #6c757d; font-size: 12px;'>Alquiler de Vehículos - 2º DAW</p>
    </div>
</div>
</body></html>";
    }

    /// <summary>
    /// Punto único de entrada: recibe cualquier DTO de notificación, pinta su cuerpo y le aplica la plantilla base.
    /// </summary>
    public static string GenerarHtml(IEmailNotificacionDto dto) => dto switch
    {
        NotificacionEmailDto generico => GenerarDesdeGenerico(generico),
        ParteTallerEmailDto parte     => GenerarParteTaller(parte),
        // Cada compañero añade aquí su línea: AltaReservaEmailDto alta => GenerarAltaReserva(alta),
        _ => throw new NotSupportedException($"No hay plantilla definida para el DTO {dto.GetType().Name}")
    };

    private static string GenerarParteTaller(ParteTallerEmailDto dto)
    {
        var campos = new Dictionary<string, string>
        {
            ["Referencia de reserva"] = dto.CodigoReferencia,
            ["Vehículo afectado"] = dto.VehiculoOriginal,
            ["Motivo de entrada en taller"] = dto.DescripcionAveria,
            ["Resolución aplicada"] = dto.ResolucionAplicada
        };

        var cuerpo = PintarTablaYCaja(
            dto.NombreCliente,
            campos,
            dto.DetalleResolucion,
            TipoAvisoEmail.Advertencia);

        return CreateBase("Actualización de tu reserva por revisión en taller", cuerpo);
    }

    private static string GenerarDesdeGenerico(NotificacionEmailDto dto)
    {
        var cuerpo = PintarTablaYCaja(dto.NombreCliente, dto.CamposTabla, dto.MensajeDestacado, dto.TipoAviso);
        return CreateBase(dto.TituloCabecera, cuerpo);
    }

    private static string PintarTablaYCaja(
        string nombreCliente,
        IDictionary<string, string> campos,
        string mensajeDestacado,
        TipoAvisoEmail tipoAviso)
    {
        var sb = new StringBuilder();
        sb.Append($"<p>Hola <strong>{Encode(nombreCliente)}</strong>,</p>");

        if (campos.Count > 0)
        {
            sb.Append("<table style='width: 100%; border-collapse: collapse; margin: 20px 0;'>");
            foreach (var (clave, valor) in campos)
            {
                sb.Append($@"
                    <tr>
                        <td style='padding: 12px; background: #f8f9fa; font-weight: 600; border-bottom: 1px solid #e9ecef;'>{Encode(clave)}:</td>
                        <td style='padding: 12px; border-bottom: 1px solid #e9ecef;'>{Encode(valor)}</td>
                    </tr>");
            }
            sb.Append("</table>");
        }

        if (!string.IsNullOrWhiteSpace(mensajeDestacado))
        {
            var (bg, border, text) = tipoAviso switch
            {
                TipoAvisoEmail.Exito       => ("#d4edda", "#28a745", "#155724"),
                TipoAvisoEmail.Advertencia => ("#fff3cd", "#ffc107", "#856404"),
                TipoAvisoEmail.Peligro     => ("#f8d7da", "#dc3545", "#721c24"),
                _                          => ("#e7f3ff", "#667eea", "#1a1a2e")
            };

            sb.Append($@"
                <p style='padding: 15px; background: {bg}; border-left: 4px solid {border}; border-radius: 4px; color: {text};'>
                    {Encode(mensajeDestacado)}
                </p>");
        }

        return sb.ToString();
    }

    private static string Encode(string? text) => WebUtility.HtmlEncode(text ?? string.Empty);
}