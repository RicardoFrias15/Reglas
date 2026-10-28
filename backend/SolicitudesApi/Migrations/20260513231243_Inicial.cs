using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolicitudesApi.Migrations
{
    public partial class Inicial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntraObjectId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NombreCompleto = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UltimoAccesoEn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Solicitudes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Folio = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Urgencia = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Estatus = table.Column<int>(type: "int", nullable: false),
                    AdministracionBeneficiada = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PersonaBeneficiada = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NombreAdministrador = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    QuienAutoriza = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Tema = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Subtema = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Objetivo = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Justificacion = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    ResultadosEsperados = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    DescripcionDetallada = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    SolicitanteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EjecutorAsignadoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solicitudes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Solicitudes_Usuarios_EjecutorAsignadoId",
                        column: x => x.EjecutorAsignadoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitudes_Usuarios_SolicitanteId",
                        column: x => x.SolicitanteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Documentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SolicitudId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: false),
                    BlobPath = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    SubidoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubidoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documentos_Solicitudes_SolicitudId",
                        column: x => x.SolicitudId,
                        principalTable: "Solicitudes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Documentos_Usuarios_SubidoPorId",
                        column: x => x.SubidoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HistorialEstatus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SolicitudId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstatusAnterior = table.Column<int>(type: "int", nullable: false),
                    EstatusNuevo = table.Column<int>(type: "int", nullable: false),
                    CambiadoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Comentario = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CambiadoEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialEstatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialEstatus_Solicitudes_SolicitudId",
                        column: x => x.SolicitudId,
                        principalTable: "Solicitudes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HistorialEstatus_Usuarios_CambiadoPorId",
                        column: x => x.CambiadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Mensajes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SolicitudId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AutorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Contenido = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mensajes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mensajes_Solicitudes_SolicitudId",
                        column: x => x.SolicitudId,
                        principalTable: "Solicitudes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Mensajes_Usuarios_AutorId",
                        column: x => x.AutorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_SolicitudId",
                table: "Documentos",
                column: "SolicitudId");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_SubidoPorId",
                table: "Documentos",
                column: "SubidoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEstatus_CambiadoPorId",
                table: "HistorialEstatus",
                column: "CambiadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEstatus_SolicitudId",
                table: "HistorialEstatus",
                column: "SolicitudId");

            migrationBuilder.CreateIndex(
                name: "IX_Mensajes_AutorId",
                table: "Mensajes",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Mensajes_SolicitudId_CreadoEn",
                table: "Mensajes",
                columns: new[] { "SolicitudId", "CreadoEn" });

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_EjecutorAsignadoId",
                table: "Solicitudes",
                column: "EjecutorAsignadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_Estatus",
                table: "Solicitudes",
                column: "Estatus");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_FechaCreacion",
                table: "Solicitudes",
                column: "FechaCreacion");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_Folio",
                table: "Solicitudes",
                column: "Folio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_SolicitanteId",
                table: "Solicitudes",
                column: "SolicitanteId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_EntraObjectId",
                table: "Usuarios",
                column: "EntraObjectId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Documentos");

            migrationBuilder.DropTable(
                name: "HistorialEstatus");

            migrationBuilder.DropTable(
                name: "Mensajes");

            migrationBuilder.DropTable(
                name: "Solicitudes");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
