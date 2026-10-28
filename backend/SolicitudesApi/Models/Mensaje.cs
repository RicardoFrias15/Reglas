namespace SolicitudesApi.Models;

public class Mensaje
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SolicitudId { get; set; }
    public Solicitud? Solicitud { get; set; }

    public Guid AutorId { get; set; }
    public AppUser? Autor { get; set; }

    public required string Contenido { get; set; }
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}
