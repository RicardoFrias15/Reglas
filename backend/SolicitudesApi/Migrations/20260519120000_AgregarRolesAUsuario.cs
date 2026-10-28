using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolicitudesApi.Migrations
{
    public partial class AgregarRolesAUsuario : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Roles",
                table: "Usuarios",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Roles",
                table: "Usuarios");
        }
    }
}
