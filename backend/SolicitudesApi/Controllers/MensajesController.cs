using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolicitudesApi.Data;
using SolicitudesApi.DTOs;
using SolicitudesApi.Models;
using SolicitudesApi.Services;

namespace SolicitudesApi.Controllers;

[ApiController]
[Route("api/solicitudes/{solicitudId:guid}/mensajes")]
[Authorize]
public class MensajesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IEmailService _email;

    public MensajesController(AppDbContext db, ICurrentUserService current, IEmailService email)
    {
        _db = db;
        _current = current;
        _email = email;
    }

    [HttpPost]
    public async Task<IActionResult> Crear(Guid solicitudId, [FromBody] CrearMensajeDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Contenido)) return BadRequest("Mensaje vacío.");

        var me = await _current.EnsureLocalUserAsync(ct);
        var s = await _db.Solicitudes
            .Include(x => x.Solicitante)
            .Include(x => x.EjecutorAsignado)
            .FirstOrDefaultAsync(x => x.Id == solicitudId, ct);
        if (s is null) return NotFound();

        var puedeHablar = s.SolicitanteId == me.Id
            || s.EjecutorAsignadoId == me.Id
            || _current.EsGestor;
        if (!puedeHablar) return Forbid();

        var msg = new Mensaje
        {
            SolicitudId = s.Id,
            AutorId = me.Id,
            Contenido = dto.Contenido
        };
        _db.Mensajes.Add(msg);
        s.FechaActualizacion = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        AppUser? destino = null;
        if (me.Id == s.SolicitanteId && s.EjecutorAsignado is not null)
            destino = s.EjecutorAsignado;
        else if (me.Id != s.SolicitanteId)
            destino = s.Solicitante;

        if (destino is not null)
        {
            await _email.SendAsync(destino.Email, destino.NombreCompleto,
                $"Nuevo mensaje en solicitud {s.Folio}",
                EmailTemplates.NuevoMensaje(s, destino.NombreCompleto, dto.Contenido), ct);
        }

        return Ok(new { msg.Id });
    }
}
