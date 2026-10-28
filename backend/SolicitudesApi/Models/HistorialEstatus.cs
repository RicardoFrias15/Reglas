namespace SolicitudesApi.Models;

public class HistorialEstatus
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SolicitudId { get; set; }
    public Solicitud? Solicitud { get; set; }

    public EstatusSolicitud EstatusAnterior { get; set; }
    public EstatusSolicitud EstatusNuevo { get; set; }

    public Guid CambiadoPorId { get; set; }
    public AppUser? CambiadoPor { get; set; }

    public string? Comentario { get; set; }
    public DateTime CambiadoEn { get; set; } = DateTime.UtcNow;
}
