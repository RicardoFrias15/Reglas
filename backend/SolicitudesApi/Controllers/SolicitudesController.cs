using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolicitudesApi.Authorization;
using SolicitudesApi.Data;
using SolicitudesApi.DTOs;
using SolicitudesApi.Models;
using SolicitudesApi.Services;

namespace SolicitudesApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SolicitudesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IBlobStorageService _blob;
    private readonly IEmailService _email;

    public SolicitudesController(AppDbContext db, ICurrentUserService current, IBlobStorageService blob, IEmailService email)
    {
        _db = db;
        _current = current;
        _blob = blob;
        _email = email;
    }

    [HttpGet]
    public async Task<ActionResult<List<SolicitudResumenDto>>> Listar(
        [FromQuery] EstatusSolicitud? estatus,
        [FromQuery] UrgenciaSolicitud? urgencia,
        CancellationToken ct = default)
    {
        var me = await _current.EnsureLocalUserAsync(ct);

        var q = _db.Solicitudes
            .Include(s => s.Solicitante)
            .Include(s => s.EjecutorAsignado)
            .AsQueryable();

        if (_current.EsAdministrador)
        {
        }
        else if (_current.EsGestor)
        {
            q = q.Where(s =>
                s.Estatus == EstatusSolicitud.Recibido ||
                s.Estatus == EstatusSolicitud.SolicitudDevuelta);
        }
        else if (_current.EsAsignadorEjecutor)
        {
            q = q.Where(s =>
                s.Estatus == EstatusSolicitud.EnEsperaAsignacionEjecutor ||
                s.Estatus == EstatusSolicitud.ResolviendoSolucion);
        }
        else if (_current.EsEjecutor)
        {
            q = q.Where(s => s.EjecutorAsignadoId == me.Id);
        }
        else
        {
            q = q.Where(s => s.SolicitanteId == me.Id);
        }

        if (estatus.HasValue) q = q.Where(s => s.Estatus == estatus.Value);
        if (urgencia.HasValue) q = q.Where(s => s.Urgencia == urgencia.Value);

        var lista = await q
            .OrderByDescending(s => s.FechaCreacion)
            .Select(s => new SolicitudResumenDto(
                s.Id, s.Folio, s.FechaCreacion, s.FechaActualizacion,
                s.Urgencia, s.Tipo, s.Estatus, s.Tema,
                s.Solicitante!.NombreCompleto,
                s.EjecutorAsignado != null ? s.EjecutorAsignado.NombreCompleto : null))
            .ToListAsync(ct);

        return Ok(lista);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SolicitudDetalleDto>> Obtener(Guid id, CancellationToken ct = default)
    {
        var me = await _current.EnsureLocalUserAsync(ct);

        var s = await _db.Solicitudes
            .Include(x => x.Solicitante)
            .Include(x => x.EjecutorAsignado)
            .Include(x => x.Documentos).ThenInclude(d => d.SubidoPor)
            .Include(x => x.Mensajes).ThenInclude(m => m.Autor)
            .Include(x => x.Historial).ThenInclude(h => h.CambiadoPor)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (s is null) return NotFound();

        var puedeVer =
            _current.EsAdministrador ||
            _current.EsGestor ||
            _current.EsAsignadorEjecutor ||
            (_current.EsEjecutor && s.EjecutorAsignadoId == me.Id) ||
            s.SolicitanteId == me.Id;
        if (!puedeVer) return Forbid();

        var docs = s.Documentos
            .OrderByDescending(d => d.SubidoEn)
            .Select(d => new DocumentoDto(
                d.Id, d.Titulo, d.Descripcion, d.NombreArchivo, d.ContentType, d.TamanoBytes, d.SubidoEn,
                _blob.GenerateReadSasUri(d.BlobPath, TimeSpan.FromMinutes(15)).ToString(),
                d.SubidoPor!.NombreCompleto))
            .ToList();

        var msgs = s.Mensajes
            .OrderBy(m => m.CreadoEn)
            .Select(m => new MensajeDto(m.Id, m.Contenido, m.CreadoEn, m.Autor!.NombreCompleto))
            .ToList();

        var hist = s.Historial
            .OrderByDescending(h => h.CambiadoEn)
            .Select(h => new HistorialDto(h.Id, h.EstatusAnterior, h.EstatusNuevo, h.Comentario, h.CambiadoEn, h.CambiadoPor!.NombreCompleto))
            .ToList();

        return Ok(new SolicitudDetalleDto(
            s.Id, s.Folio, s.FechaCreacion, s.FechaActualizacion,
            s.Urgencia, s.Tipo, s.Estatus,
            s.AdministracionBeneficiada, s.PersonaBeneficiada, s.NombreAdministrador, s.QuienAutoriza,
            s.Tema, s.Subtema, s.Objetivo, s.Justificacion, s.ResultadosEsperados, s.DescripcionDetallada,
            s.Solicitante!.NombreCompleto, s.Solicitante.Email, s.SolicitanteId,
            s.EjecutorAsignado?.NombreCompleto, s.EjecutorAsignadoId,
            docs, msgs, hist));
    }

    [HttpPost]
    [Authorize(Policy = AppRoles.Usuario)]
    public async Task<ActionResult<Guid>> Crear([FromBody] CrearSolicitudDto dto, CancellationToken ct = default)
    {
        var me = await _current.EnsureLocalUserAsync(ct);

        var s = new Solicitud
        {
            Folio = await GenerarFolioAsync(ct),
            Urgencia = dto.Urgencia,
            Tipo = dto.Tipo,
            Estatus = EstatusSolicitud.Recibido,
            AdministracionBeneficiada = dto.AdministracionBeneficiada,
            PersonaBeneficiada = dto.PersonaBeneficiada,
            NombreAdministrador = dto.NombreAdministrador,
            QuienAutoriza = dto.QuienAutoriza,
            Tema = dto.Tema,
            Subtema = dto.Subtema,
            Objetivo = dto.Objetivo,
            Justificacion = dto.Justificacion,
            ResultadosEsperados = dto.ResultadosEsperados,
            DescripcionDetallada = dto.DescripcionDetallada,
            SolicitanteId = me.Id
        };
        s.Historial.Add(new HistorialEstatus
        {
            EstatusAnterior = EstatusSolicitud.Recibido,
            EstatusNuevo = EstatusSolicitud.Recibido,
            CambiadoPorId = me.Id,
            Comentario = "Solicitud creada"
        });

        _db.Solicitudes.Add(s);
        await _db.SaveChangesAsync(ct);

        await _email.SendAsync(me.Email, me.NombreCompleto,
            $"Solicitud {s.Folio} recibida",
            EmailTemplates.CambioEstatus(s, me.NombreCompleto, "Hemos recibido tu solicitud."), ct);

        return CreatedAtAction(nameof(Obtener), new { id = s.Id }, s.Id);
    }

    [HttpPut("{id:guid}/contenido")]
    [Authorize(Policy = AppRoles.Usuario)]
    public async Task<IActionResult> ActualizarContenidoUsuario(Guid id, [FromBody] ActualizarSolicitudUsuarioDto dto, CancellationToken ct = default)
    {
        var me = await _current.EnsureLocalUserAsync(ct);

        var s = await _db.Solicitudes
            .Include(x => x.EjecutorAsignado)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (s is null) return NotFound();
        if (s.SolicitanteId != me.Id) return Forbid();
        if (s.Estatus != EstatusSolicitud.SolicitudDevuelta)
            return BadRequest("Solo puedes editar la solicitud cuando el Gestor te ha devuelto la solicitud.");

        var anterior = s.Estatus;
        var nuevaDescripcion = string.IsNullOrWhiteSpace(dto.DescripcionDetallada) ? s.DescripcionDetallada : dto.DescripcionDetallada!;
        var nuevaJustificacion = string.IsNullOrWhiteSpace(dto.Justificacion) ? s.Justificacion : dto.Justificacion!;
        var ahora = DateTime.UtcNow;

        var rows = await _db.Solicitudes
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(set => set
                .SetProperty(x => x.DescripcionDetallada, nuevaDescripcion)
                .SetProperty(x => x.Justificacion, nuevaJustificacion)
                .SetProperty(x => x.Estatus, EstatusSolicitud.Recibido)
                .SetProperty(x => x.FechaActualizacion, ahora), ct);

        if (rows == 0) return NotFound();

        _db.HistorialEstatus.Add(new HistorialEstatus
        {
            SolicitudId = id,
            EstatusAnterior = anterior,
            EstatusNuevo = EstatusSolicitud.Recibido,
            CambiadoPorId = me.Id,
            Comentario = "El usuario re-envió la solicitud con cambios."
        });
        await _db.SaveChangesAsync(ct);

        if (s.EjecutorAsignado is not null)
        {
            s.Estatus = EstatusSolicitud.Recibido;
            await _email.SendAsync(s.EjecutorAsignado.Email, s.EjecutorAsignado.NombreCompleto,
                $"Solicitud {s.Folio} actualizada",
                EmailTemplates.CambioEstatus(s, s.EjecutorAsignado.NombreCompleto, "El usuario actualizó la solicitud."), ct);
        }
        return NoContent();
    }

    [HttpPost("{id:guid}/aprobar")]
    [Authorize(Policy = AppRoles.Gestor)]
    public Task<IActionResult> Aprobar(Guid id, [FromBody] CambiarEstatusDto dto, CancellationToken ct = default) =>
        CambiarEstatusGestionado(
            id,
            dto.Comentario,
            from: [EstatusSolicitud.Recibido, EstatusSolicitud.SolicitudDevuelta],
            to: EstatusSolicitud.EnEsperaAsignacionEjecutor,
            mensajeHistorialDefault: "El gestor aprobó la solicitud.",
            ct: ct);

    [HttpPost("{id:guid}/devolver")]
    [Authorize(Policy = AppRoles.Gestor)]
    public Task<IActionResult> Devolver(Guid id, [FromBody] CambiarEstatusDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Comentario))
            return Task.FromResult<IActionResult>(BadRequest("Debes indicar el motivo de la devolución en el comentario."));

        return CambiarEstatusGestionado(
            id,
            dto.Comentario,
            from: [EstatusSolicitud.Recibido],
            to: EstatusSolicitud.SolicitudDevuelta,
            mensajeHistorialDefault: "El gestor devolvió la solicitud al usuario.",
            ct: ct);
    }

    [HttpPost("{id:guid}/finalizar")]
    [Authorize(Policy = AppRoles.Ejecutor)]
    public Task<IActionResult> Finalizar(Guid id, [FromBody] CambiarEstatusDto dto, CancellationToken ct = default) =>
        CambiarEstatusGestionado(
            id,
            dto.Comentario,
            from: [EstatusSolicitud.ResolviendoSolucion],
            to: EstatusSolicitud.Finalizado,
            mensajeHistorialDefault: "El ejecutor finalizó la solicitud.",
            soloEjecutorAsignado: true,
            ct: ct);

    [HttpPost("{id:guid}/cancelar")]
    [Authorize(Policy = AppRoles.Ejecutor)]
    public Task<IActionResult> Cancelar(Guid id, [FromBody] CambiarEstatusDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Comentario))
            return Task.FromResult<IActionResult>(BadRequest("Debes indicar el motivo de la cancelación en el comentario."));

        return CambiarEstatusGestionado(
            id,
            dto.Comentario,
            from: [EstatusSolicitud.ResolviendoSolucion],
            to: EstatusSolicitud.Cancelado,
            mensajeHistorialDefault: "El ejecutor canceló la solicitud.",
            soloEjecutorAsignado: true,
            ct: ct);
    }

    private async Task<IActionResult> CambiarEstatusGestionado(
        Guid id,
        string? comentario,
        EstatusSolicitud[] from,
        EstatusSolicitud to,
        string mensajeHistorialDefault,
        CancellationToken ct,
        bool soloEjecutorAsignado = false)
    {
        var me = await _current.EnsureLocalUserAsync(ct);

        var s = await _db.Solicitudes
            .Include(x => x.Solicitante)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (s is null) return NotFound();

        if (!from.Contains(s.Estatus))
            return BadRequest($"La solicitud está en estatus {s.Estatus} y no permite esta acción.");

        if (soloEjecutorAsignado && !_current.EsAdministrador && s.EjecutorAsignadoId != me.Id)
            return Forbid();

        var anterior = s.Estatus;
        var ahora = DateTime.UtcNow;

        var rows = await _db.Solicitudes
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(set => set
                .SetProperty(x => x.Estatus, to)
                .SetProperty(x => x.FechaActualizacion, ahora), ct);

        if (rows == 0) return NotFound();

        _db.HistorialEstatus.Add(new HistorialEstatus
        {
            SolicitudId = id,
            EstatusAnterior = anterior,
            EstatusNuevo = to,
            CambiadoPorId = me.Id,
            Comentario = string.IsNullOrWhiteSpace(comentario) ? mensajeHistorialDefault : comentario
        });
        await _db.SaveChangesAsync(ct);

        s.Estatus = to;
        await _email.SendAsync(s.Solicitante!.Email, s.Solicitante.NombreCompleto,
            $"Solicitud {s.Folio}: {to}",
            EmailTemplates.CambioEstatus(s, s.Solicitante.NombreCompleto, comentario), ct);

        return NoContent();
    }

    [HttpPost("{id:guid}/asignar")]
    [Authorize(Policy = AppRoles.AsignadorEjecutor)]
    public async Task<IActionResult> AsignarEjecutor(Guid id, [FromBody] AsignarEjecutorDto dto, CancellationToken ct = default)
    {
        var me = await _current.EnsureLocalUserAsync(ct);

        var s = await _db.Solicitudes
            .Include(x => x.Solicitante)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (s is null) return NotFound();

        if (s.Estatus != EstatusSolicitud.EnEsperaAsignacionEjecutor &&
            s.Estatus != EstatusSolicitud.ResolviendoSolucion)
        {
            return BadRequest($"No se puede asignar ejecutor en estatus {s.Estatus}.");
        }

        var ejecutor = await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == dto.EjecutorId, ct);
        if (ejecutor is null) return BadRequest("Ejecutor no encontrado.");

        var rolesEjecutor = (ejecutor.Roles ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (!rolesEjecutor.Contains(AppRoles.Ejecutor) && !rolesEjecutor.Contains(AppRoles.Administrador))
        {
            return BadRequest($"{ejecutor.NombreCompleto} no tiene el rol Ejecutor.");
        }

        var anterior = s.Estatus;
        var ahora = DateTime.UtcNow;

        var rows = await _db.Solicitudes
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(set => set
                .SetProperty(x => x.EjecutorAsignadoId, ejecutor.Id)
                .SetProperty(x => x.Estatus, EstatusSolicitud.ResolviendoSolucion)
                .SetProperty(x => x.FechaActualizacion, ahora), ct);

        if (rows == 0) return NotFound();

        _db.HistorialEstatus.Add(new HistorialEstatus
        {
            SolicitudId = id,
            EstatusAnterior = anterior,
            EstatusNuevo = EstatusSolicitud.ResolviendoSolucion,
            CambiadoPorId = me.Id,
            Comentario = $"Asignado a {ejecutor.NombreCompleto}. Estatus → ResolviendoSolucion."
        });
        await _db.SaveChangesAsync(ct);

        await _email.SendAsync(ejecutor.Email, ejecutor.NombreCompleto,
            $"Se te asignó la solicitud {s.Folio}",
            EmailTemplates.CambioEstatus(s, ejecutor.NombreCompleto, "Se te ha asignado esta solicitud."), ct);

        return NoContent();
    }

    private async Task<string> GenerarFolioAsync(CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"SOL-{year}-";
        var ultima = await _db.Solicitudes
            .Where(s => s.Folio.StartsWith(prefix))
            .OrderByDescending(s => s.Folio)
            .Select(s => s.Folio)
            .FirstOrDefaultAsync(ct);

        var siguiente = 1;
        if (!string.IsNullOrEmpty(ultima) && int.TryParse(ultima[prefix.Length..], out var n))
            siguiente = n + 1;

        return $"{prefix}{siguiente:D6}";
    }
}
