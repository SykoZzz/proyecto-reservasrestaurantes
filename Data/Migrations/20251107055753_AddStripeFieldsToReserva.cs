using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace appReservas.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStripeFieldsToReserva : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "estado_pago",
                table: "reservas",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "precio",
                table: "reservas",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "stripe_session_id",
                table: "reservas",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "estado_pago",
                table: "reservas");

            migrationBuilder.DropColumn(
                name: "precio",
                table: "reservas");

            migrationBuilder.DropColumn(
                name: "stripe_session_id",
                table: "reservas");
        }
    }
}
