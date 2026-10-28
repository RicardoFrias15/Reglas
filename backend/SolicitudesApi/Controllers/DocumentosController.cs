using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolicitudesApi.Data;
using SolicitudesApi.Models;
using SolicitudesApi.Services;

namespace SolicitudesApi.Controllers;

[ApiController]
[Route("api/solicitudes/{solicitudId:guid}/documentos")]
[Authorize]
public class DocumentosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IBlobStorageService _blob;
    private readonly IConfiguration _cfg;

    public DocumentosController(AppDbContext db, ICurrentUserService current, IBlobStorageService blob, IConfiguration cfg)
    {
        _db = db;
        _current = current;
        _blob = blob;
        _cfg = cfg;
    }

    [HttpPost]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> Subir(
        Guid solicitudId,
        [FromForm] string titulo,
        [FromForm] string descripcion,
        IFormFile archivo,
        CancellationToken ct = default)
    {
        if (archivo is null || archivo.Length == 0) return BadRequest("Falta el archivo.");
        if (string.IsNullOrWhiteSpace(titulo)) return BadRequest("Falta el título.");
        if (string.IsNullOrWhiteSpace(descripcion)) return BadRequest("Falta la descripción.");

        var allowed = _cfg.GetSection("App:AllowedFileExtensions").Get<string[]>() ?? [".pdf", ".doc", ".docx"];
        var maxMb = _cfg.GetValue<int>("App:MaxFileSizeMb", 25);

        var ext = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext))
            return BadRequest($"Extensión no permitida. Solo: {string.Join(", ", allowed)}");
        if (archivo.Length > maxMb * 1024L * 1024L)
            return BadRequest($"Archivo excede el máximo de {maxMb} MB.");

        var me = await _current.EnsureLocalUserAsync(ct);
        var solicitud = await _db.Solicitudes
            .Include(s => s.Documentos)
            .FirstOrDefaultAsync(s => s.Id == solicitudId, ct);
        if (solicitud is null) return NotFound();

        var esEjecutorAsignado = solicitud.EjecutorAsignadoId.HasValue && solicitud.EjecutorAsignadoId == me.Id;
        if (solicitud.SolicitanteId != me.Id && !esEjecutorAsignado && !_current.EsAdministrador)
            return Forbid();

        var previo = solicitud.Documentos.FirstOrDefault(d => d.Titulo == titulo);
        if (previo is not null)
        {
            await _blob.DeleteAsync(previo.BlobPath, ct);
            _db.Documentos.Remove(previo);
        }

        await using var stream = archivo.OpenReadStream();
        var blobPath = await _blob.UploadAsync(stream, archivo.FileName, archivo.ContentType, ct);

        var doc = new Documento
        {
            SolicitudId = solicitud.Id,
            Titulo = titulo,
            Descripcion = descripcion,
            NombreArchivo = archivo.FileName,
            ContentType = archivo.ContentType,
            TamanoBytes = archivo.Length,
            BlobPath = blobPath,
            SubidoPorId = me.Id
        };
        _db.Documentos.Add(doc);
        solicitud.FechaActualizacion = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return Ok(new { doc.Id });
    }

    [HttpDelete("{docId:guid}")]
    public async Task<IActionResult> Eliminar(Guid solicitudId, Guid docId, CancellationToken ct = default)
    {
        var me = await _current.EnsureLocalUserAsync(ct);
        var doc = await _db.Documentos
            .Include(d => d.Solicitud)
            .FirstOrDefaultAsync(d => d.Id == docId && d.SolicitudId == solicitudId, ct);
        if (doc is null) return NotFound();
        if (doc.Solicitud!.SolicitanteId != me.Id && !_current.EsAdministrador) return Forbid();

        await _blob.DeleteAsync(doc.BlobPath, ct);
        _db.Documentos.Remove(doc);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
