using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SolicitudesApi.Authorization;
using SolicitudesApi.Data;
using SolicitudesApi.Models;

namespace SolicitudesApi.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _http;
    private readonly AppDbContext _db;

    public CurrentUserService(IHttpContextAccessor http, AppDbContext db)
    {
        _http = http;
        _db = db;
    }

    private ClaimsPrincipal? User => _http.HttpContext?.User;

    public string EntraObjectId =>
        User?.FindFirst("oid")?.Value
        ?? User?.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
        ?? throw new InvalidOperationException("No hay claim 'oid' en el token.");

    public string Email =>
        User?.FindFirst("preferred_username")?.Value
        ?? User?.FindFirst(ClaimTypes.Upn)?.Value
        ?? User?.FindFirst(ClaimTypes.Email)?.Value
        ?? string.Empty;

    public string NombreCompleto =>
        User?.FindFirst("name")?.Value
        ?? User?.Identity?.Name
        ?? Email;

    public IReadOnlyList<string> Roles =>
        User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
        ?? new List<string>();

    public bool EsAdministrador => Roles.Contains(AppRoles.Administrador);
    public bool EsGestor => EsAdministrador || Roles.Contains(AppRoles.Gestor);
    public bool EsAsignadorEjecutor => EsAdministrador || Roles.Contains(AppRoles.AsignadorEjecutor);
    public bool EsEjecutor => EsAdministrador || Roles.Contains(AppRoles.Ejecutor);

    public async Task<AppUser> EnsureLocalUserAsync(CancellationToken ct = default)
    {
        var oid = EntraObjectId;
        var rolesCsv = string.Join(",", Roles);

        var user = await _db.Usuarios.FirstOrDefaultAsync(u => u.EntraObjectId == oid, ct);
        if (user is null)
        {
            user = new AppUser
            {
                EntraObjectId = oid,
                Email = Email,
                NombreCompleto = NombreCompleto,
                Roles = rolesCsv
            };
            _db.Usuarios.Add(user);
        }
        else
        {
            user.Email = Email;
            user.NombreCompleto = NombreCompleto;
            user.Roles = rolesCsv;
        }
        user.UltimoAccesoEn = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return user;
    }
}
