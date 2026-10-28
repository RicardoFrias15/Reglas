using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolicitudesApi.Authorization;
using SolicitudesApi.Data;
using SolicitudesApi.DTOs;
using SolicitudesApi.Services;

namespace SolicitudesApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _current;

    public UsuariosController(AppDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct = default)
    {
        var me = await _current.EnsureLocalUserAsync(ct);
        return Ok(new
        {
            me.Id,
            me.Email,
            me.NombreCompleto,
            Roles = _current.Roles
        });
    }

    [HttpGet("ejecutores")]
    [Authorize(Policy = AppRoles.AsignadorEjecutor)]
    public async Task<ActionResult<List<UsuarioDto>>> Ejecutores(CancellationToken ct = default)
    {
        var lista = await _db.Usuarios
            .Where(u => ("," + u.Roles + ",").Contains("," + AppRoles.Ejecutor + ","))
            .OrderBy(u => u.NombreCompleto)
            .Select(u => new UsuarioDto(u.Id, u.NombreCompleto, u.Email))
            .ToListAsync(ct);
        return Ok(lista);
    }
}
