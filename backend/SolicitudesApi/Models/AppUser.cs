namespace SolicitudesApi.Models;

public class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string EntraObjectId { get; set; }

    public required string Email { get; set; }
    public required string NombreCompleto { get; set; }

    public string Roles { get; set; } = string.Empty;

    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public DateTime? UltimoAccesoEn { get; set; }
}
