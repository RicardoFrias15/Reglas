using Microsoft.EntityFrameworkCore;
using SolicitudesApi.Models;

namespace SolicitudesApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AppUser> Usuarios => Set<AppUser>();
    public DbSet<Solicitud> Solicitudes => Set<Solicitud>();
    public DbSet<Documento> Documentos => Set<Documento>();
    public DbSet<Mensaje> Mensajes => Set<Mensaje>();
    public DbSet<HistorialEstatus> HistorialEstatus => Set<HistorialEstatus>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<AppUser>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.EntraObjectId).IsUnique();
            e.Property(x => x.EntraObjectId).HasMaxLength(64).IsRequired();
            e.Property(x => x.Email).HasMaxLength(256).IsRequired();
            e.Property(x => x.NombreCompleto).HasMaxLength(256).IsRequired();
            e.Property(x => x.Roles).HasMaxLength(256).HasDefaultValue(string.Empty);
        });

        b.Entity<Solicitud>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Folio).IsUnique();
            e.Property(x => x.Folio).HasMaxLength(32).IsRequired();
            e.Property(x => x.AdministracionBeneficiada).HasMaxLength(256);
            e.Property(x => x.PersonaBeneficiada).HasMaxLength(256);
            e.Property(x => x.NombreAdministrador).HasMaxLength(256);
            e.Property(x => x.QuienAutoriza).HasMaxLength(256);
            e.Property(x => x.Tema).HasMaxLength(256);
            e.Property(x => x.Subtema).HasMaxLength(256);
            e.Property(x => x.Objetivo).HasMaxLength(1024);
            e.Property(x => x.Justificacion).HasMaxLength(2048);
            e.Property(x => x.ResultadosEsperados).HasMaxLength(2048);
            e.Property(x => x.DescripcionDetallada).HasMaxLength(4000);

            e.HasOne(x => x.Solicitante)
                .WithMany()
                .HasForeignKey(x => x.SolicitanteId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.EjecutorAsignado)
                .WithMany()
                .HasForeignKey(x => x.EjecutorAsignadoId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.Estatus);
            e.HasIndex(x => x.FechaCreacion);
        });

        b.Entity<Documento>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Titulo).HasMaxLength(256).IsRequired();
            e.Property(x => x.Descripcion).HasMaxLength(2000).IsRequired();
            e.Property(x => x.NombreArchivo).HasMaxLength(512).IsRequired();
            e.Property(x => x.ContentType).HasMaxLength(128).IsRequired();
            e.Property(x => x.BlobPath).HasMaxLength(1024).IsRequired();

            e.HasOne(x => x.Solicitud)
                .WithMany(s => s.Documentos)
                .HasForeignKey(x => x.SolicitudId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.SubidoPor)
                .WithMany()
                .HasForeignKey(x => x.SubidoPorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Mensaje>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Contenido).HasMaxLength(4000).IsRequired();

            e.HasOne(x => x.Solicitud)
                .WithMany(s => s.Mensajes)
                .HasForeignKey(x => x.SolicitudId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Autor)
                .WithMany()
                .HasForeignKey(x => x.AutorId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.SolicitudId, x.CreadoEn });
        });

        b.Entity<HistorialEstatus>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Comentario).HasMaxLength(1000);

            e.HasOne(x => x.Solicitud)
                .WithMany(s => s.Historial)
                .HasForeignKey(x => x.SolicitudId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.CambiadoPor)
                .WithMany()
                .HasForeignKey(x => x.CambiadoPorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(b);
    }
}
