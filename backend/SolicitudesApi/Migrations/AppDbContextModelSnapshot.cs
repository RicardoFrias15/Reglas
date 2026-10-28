using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SolicitudesApi.Data;

#nullable disable

namespace SolicitudesApi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("SolicitudesApi.Models.AppUser", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreadoEn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("EntraObjectId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("NombreCompleto")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("Roles")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)")
                        .HasDefaultValue("");

                    b.Property<DateTime?>("UltimoAccesoEn")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("EntraObjectId")
                        .IsUnique();

                    b.ToTable("Usuarios");
                });

            modelBuilder.Entity("SolicitudesApi.Models.Documento", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("BlobPath")
                        .IsRequired()
                        .HasMaxLength(1024)
                        .HasColumnType("nvarchar(1024)");

                    b.Property<string>("ContentType")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Descripcion")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("nvarchar(2000)");

                    b.Property<string>("NombreArchivo")
                        .IsRequired()
                        .HasMaxLength(512)
                        .HasColumnType("nvarchar(512)");

                    b.Property<Guid>("SolicitudId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("SubidoEn")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("SubidoPorId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<long>("TamanoBytes")
                        .HasColumnType("bigint");

                    b.Property<string>("Titulo")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("SolicitudId");

                    b.HasIndex("SubidoPorId");

                    b.ToTable("Documentos");
                });

            modelBuilder.Entity("SolicitudesApi.Models.HistorialEstatus", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CambiadoEn")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("CambiadoPorId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Comentario")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<int>("EstatusAnterior")
                        .HasColumnType("int");

                    b.Property<int>("EstatusNuevo")
                        .HasColumnType("int");

                    b.Property<Guid>("SolicitudId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("CambiadoPorId");

                    b.HasIndex("SolicitudId");

                    b.ToTable("HistorialEstatus");
                });

            modelBuilder.Entity("SolicitudesApi.Models.Mensaje", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("AutorId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Contenido")
                        .IsRequired()
                        .HasMaxLength(4000)
                        .HasColumnType("nvarchar(4000)");

                    b.Property<DateTime>("CreadoEn")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("SolicitudId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("AutorId");

                    b.HasIndex("SolicitudId", "CreadoEn");

                    b.ToTable("Mensajes");
                });

            modelBuilder.Entity("SolicitudesApi.Models.Solicitud", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AdministracionBeneficiada")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("DescripcionDetallada")
                        .IsRequired()
                        .HasMaxLength(4000)
                        .HasColumnType("nvarchar(4000)");

                    b.Property<Guid?>("EjecutorAsignadoId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Estatus")
                        .HasColumnType("int");

                    b.Property<DateTime>("FechaActualizacion")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("FechaCreacion")
                        .HasColumnType("datetime2");

                    b.Property<string>("Folio")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)");

                    b.Property<string>("Justificacion")
                        .IsRequired()
                        .HasMaxLength(2048)
                        .HasColumnType("nvarchar(2048)");

                    b.Property<string>("NombreAdministrador")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("Objetivo")
                        .IsRequired()
                        .HasMaxLength(1024)
                        .HasColumnType("nvarchar(1024)");

                    b.Property<string>("PersonaBeneficiada")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("QuienAutoriza")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("ResultadosEsperados")
                        .IsRequired()
                        .HasMaxLength(2048)
                        .HasColumnType("nvarchar(2048)");

                    b.Property<Guid>("SolicitanteId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Subtema")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("Tema")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<int>("Tipo")
                        .HasColumnType("int");

                    b.Property<int>("Urgencia")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("EjecutorAsignadoId");

                    b.HasIndex("Estatus");

                    b.HasIndex("FechaCreacion");

                    b.HasIndex("Folio")
                        .IsUnique();

                    b.HasIndex("SolicitanteId");

                    b.ToTable("Solicitudes");
                });

            modelBuilder.Entity("SolicitudesApi.Models.Documento", b =>
                {
                    b.HasOne("SolicitudesApi.Models.Solicitud", "Solicitud")
                        .WithMany("Documentos")
                        .HasForeignKey("SolicitudId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SolicitudesApi.Models.AppUser", "SubidoPor")
                        .WithMany()
                        .HasForeignKey("SubidoPorId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Solicitud");

                    b.Navigation("SubidoPor");
                });

            modelBuilder.Entity("SolicitudesApi.Models.HistorialEstatus", b =>
                {
                    b.HasOne("SolicitudesApi.Models.AppUser", "CambiadoPor")
                        .WithMany()
                        .HasForeignKey("CambiadoPorId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("SolicitudesApi.Models.Solicitud", "Solicitud")
                        .WithMany("Historial")
                        .HasForeignKey("SolicitudId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CambiadoPor");

                    b.Navigation("Solicitud");
                });

            modelBuilder.Entity("SolicitudesApi.Models.Mensaje", b =>
                {
                    b.HasOne("SolicitudesApi.Models.AppUser", "Autor")
                        .WithMany()
                        .HasForeignKey("AutorId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("SolicitudesApi.Models.Solicitud", "Solicitud")
                        .WithMany("Mensajes")
                        .HasForeignKey("SolicitudId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Autor");

                    b.Navigation("Solicitud");
                });

            modelBuilder.Entity("SolicitudesApi.Models.Solicitud", b =>
                {
                    b.HasOne("SolicitudesApi.Models.AppUser", "EjecutorAsignado")
                        .WithMany()
                        .HasForeignKey("EjecutorAsignadoId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("SolicitudesApi.Models.AppUser", "Solicitante")
                        .WithMany()
                        .HasForeignKey("SolicitanteId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("EjecutorAsignado");

                    b.Navigation("Solicitante");
                });

            modelBuilder.Entity("SolicitudesApi.Models.Solicitud", b =>
                {
                    b.Navigation("Documentos");

                    b.Navigation("Historial");

                    b.Navigation("Mensajes");
                });
#pragma warning restore 612, 618
        }
    }
}
