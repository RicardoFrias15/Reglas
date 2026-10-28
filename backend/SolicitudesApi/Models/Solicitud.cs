namespace SolicitudesApi.Models;

public class Solicitud
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Folio { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    public UrgenciaSolicitud Urgencia { get; set; }
    public TipoSolicitud Tipo { get; set; }
    public EstatusSolicitud Estatus { get; set; } = EstatusSolicitud.Recibido;

    public required string AdministracionBeneficiada { get; set; }
    public required string PersonaBeneficiada { get; set; }
    public required string NombreAdministrador { get; set; }
    public required string QuienAutoriza { get; set; }

    public required string Tema { get; set; }
    public required string Subtema { get; set; }
    public required string Objetivo { get; set; }
    public required string Justificacion { get; set; }
    public required string ResultadosEsperados { get; set; }
    public required string DescripcionDetallada { get; set; }

    public Guid SolicitanteId { get; set; }
    public AppUser? Solicitante { get; set; }

    public Guid? EjecutorAsignadoId { get; set; }
    public AppUser? EjecutorAsignado { get; set; }

    public List<Documento> Documentos { get; set; } = [];
    public List<Mensaje> Mensajes { get; set; } = [];
    public List<HistorialEstatus> Historial { get; set; } = [];
}
