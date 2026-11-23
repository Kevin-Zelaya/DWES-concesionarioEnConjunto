using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIAlquilerVehiculos.Migrations
{
    /// <inheritdoc />
    public partial class ConfigurarCodigoReferenciaReserva : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CodigoRefencia",
                table: "Reservas",
                type: "character varying(6)",
                maxLength: 6,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CodigoRefencia",
                table: "Reservas",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(6)",
                oldMaxLength: 6,
                oldNullable: true);
        }
    }
}
