using SolicitudesApi.Models;

namespace SolicitudesApi.Services;

public static class EmailTemplates
{
    private const string Carmesi = "#990F02";

    public static string CambioEstatus(Solicitud s, string nombreDestinatario, string? comentario)
    {
        var estatus = s.Estatus.ToString();
        var comentarioHtml = string.IsNullOrWhiteSpace(comentario)
            ? string.Empty
            : $"<p style='margin-top:16px;padding:12px;background:#f7f1f1;border-left:4px solid {Carmesi};'><strong>Comentario:</strong><br/>{System.Net.WebUtility.HtmlEncode(comentario)}</p>";

        return $@"
<!DOCTYPE html>
<html><body style='font-family:Segoe UI, Arial, sans-serif; color:#222;'>
  <div style='max-width:600px;margin:0 auto;padding:24px;'>
    <h2 style='color:{Carmesi};margin:0 0 8px 0;'>Solicitudes</h2>
    <p>Hola <strong>{System.Net.WebUtility.HtmlEncode(nombreDestinatario)}</strong>,</p>
    <p>La solicitud <strong>{s.Folio}</strong> ha cambiado de estatus.</p>
    <table style='width:100%;border-collapse:collapse;margin:16px 0;'>
      <tr><td style='padding:6px 0;'><strong>Folio</strong></td><td>{s.Folio}</td></tr>
      <tr><td style='padding:6px 0;'><strong>Tema</strong></td><td>{System.Net.WebUtility.HtmlEncode(s.Tema)}</td></tr>
      <tr><td style='padding:6px 0;'><strong>Estatus</strong></td><td><span style='background:{Carmesi};color:white;padding:4px 10px;border-radius:4px;'>{estatus}</span></td></tr>
      <tr><td style='padding:6px 0;'><strong>Urgencia</strong></td><td>{s.Urgencia}</td></tr>
    </table>
    {comentarioHtml}
    <p style='margin-top:24px;font-size:12px;color:#888;'>Este correo es automático, por favor no respondas.</p>
  </div>
</body></html>";
    }

    public static string NuevoMensaje(Solicitud s, string nombreDestinatario, string mensaje)
    {
        return $@"
<!DOCTYPE html>
<html><body style='font-family:Segoe UI, Arial, sans-serif; color:#222;'>
  <div style='max-width:600px;margin:0 auto;padding:24px;'>
    <h2 style='color:{Carmesi};margin:0 0 8px 0;'>Solicitudes</h2>
    <p>Hola <strong>{System.Net.WebUtility.HtmlEncode(nombreDestinatario)}</strong>,</p>
    <p>Hay una nueva actualización en la solicitud <strong>{s.Folio}</strong>:</p>
    <blockquote style='margin:16px 0;padding:12px;background:#f7f1f1;border-left:4px solid {Carmesi};'>
      {System.Net.WebUtility.HtmlEncode(mensaje)}
    </blockquote>
    <p style='margin-top:24px;font-size:12px;color:#888;'>Este correo es automático, por favor no respondas.</p>
  </div>
</body></html>";
    }
}
