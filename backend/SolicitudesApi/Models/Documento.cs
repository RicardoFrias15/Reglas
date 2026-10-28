namespace SolicitudesApi.Models;

public class Documento
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SolicitudId { get; set; }
    public Solicitud? Solicitud { get; set; }

    public required string Titulo { get; set; }
    public required string Descripcion { get; set; }

    public required string NombreArchivo { get; set; }
    public required string ContentType { get; set; }
    public long TamanoBytes { get; set; }

    public required string BlobPath { get; set; }

    public DateTime SubidoEn { get; set; } = DateTime.UtcNow;

    public Guid SubidoPorId { get; set; }
    public AppUser? SubidoPor { get; set; }
}
