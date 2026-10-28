using SolicitudesApi.Models;

namespace SolicitudesApi.DTOs;

public record CrearSolicitudDto(
    UrgenciaSolicitud Urgencia,
    TipoSolicitud Tipo,
    string AdministracionBeneficiada,
    string PersonaBeneficiada,
    string NombreAdministrador,
    string QuienAutoriza,
    string Tema,
    string Subtema,
    string Objetivo,
    string Justificacion,
    string ResultadosEsperados,
    string DescripcionDetallada
);

public record ActualizarSolicitudUsuarioDto(
    string? DescripcionDetallada,
    string? Justificacion
);

public record CambiarEstatusDto(
    EstatusSolicitud NuevoEstatus,
    string? Comentario
);

public record AsignarEjecutorDto(Guid EjecutorId);

public record CrearMensajeDto(string Contenido);

public record DocumentoDto(
    Guid Id,
    string Titulo,
    string Descripcion,
    string NombreArchivo,
    string ContentType,
    long TamanoBytes,
    DateTime SubidoEn,
    string? UrlDescarga,
    string SubidoPor
);

public record MensajeDto(
    Guid Id,
    string Contenido,
    DateTime CreadoEn,
    string Autor
);

public record HistorialDto(
    Guid Id,
    EstatusSolicitud EstatusAnterior,
    EstatusSolicitud EstatusNuevo,
    string? Comentario,
    DateTime CambiadoEn,
    string CambiadoPor
);

public record SolicitudResumenDto(
    Guid Id,
    string Folio,
    DateTime FechaCreacion,
    DateTime FechaActualizacion,
    UrgenciaSolicitud Urgencia,
    TipoSolicitud Tipo,
    EstatusSolicitud Estatus,
    string Tema,
    string Solicitante,
    string? EjecutorAsignado
);

public record SolicitudDetalleDto(
    Guid Id,
    string Folio,
    DateTime FechaCreacion,
    DateTime FechaActualizacion,
    UrgenciaSolicitud Urgencia,
    TipoSolicitud Tipo,
    EstatusSolicitud Estatus,
    string AdministracionBeneficiada,
    string PersonaBeneficiada,
    string NombreAdministrador,
    string QuienAutoriza,
    string Tema,
    string Subtema,
    string Objetivo,
    string Justificacion,
    string ResultadosEsperados,
    string DescripcionDetallada,
    string Solicitante,
    string SolicitanteEmail,
    Guid SolicitanteId,
    string? EjecutorAsignado,
    Guid? EjecutorAsignadoId,
    List<DocumentoDto> Documentos,
    List<MensajeDto> Mensajes,
    List<HistorialDto> Historial
);

public record UsuarioDto(Guid Id, string NombreCompleto, string Email);
