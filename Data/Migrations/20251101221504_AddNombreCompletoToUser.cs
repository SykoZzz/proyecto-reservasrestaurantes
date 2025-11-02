using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace appReservas.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNombreCompletoToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "nombre_completo",
                table: "AspNetUsers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "nombre_completo",
                table: "AspNetUsers");
        }
    }
}
