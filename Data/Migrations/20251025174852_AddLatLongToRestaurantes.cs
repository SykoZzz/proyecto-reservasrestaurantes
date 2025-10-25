using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace appReservas.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLatLongToRestaurantes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "latitude",
                table: "restaurantes",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "longitude",
                table: "restaurantes",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "latitude",
                table: "restaurantes");

            migrationBuilder.DropColumn(
                name: "longitude",
                table: "restaurantes");
        }
    }
}
