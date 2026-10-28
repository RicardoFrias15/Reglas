using SolicitudesApi.Models;

namespace SolicitudesApi.Services;

public interface ICurrentUserService
{
    string EntraObjectId { get; }
    string Email { get; }
    string NombreCompleto { get; }
    IReadOnlyList<string> Roles { get; }
    bool EsAdministrador { get; }
    bool EsGestor { get; }
    bool EsAsignadorEjecutor { get; }
    bool EsEjecutor { get; }

    Task<AppUser> EnsureLocalUserAsync(CancellationToken ct = default);
}
