using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIAlquilerVehiculos.Migrations
{
    /// <inheritdoc />
    public partial class AddActualizaciónDatos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehiculos_Marcas_Marca_id",
                table: "Vehiculos");

            migrationBuilder.DropIndex(
                name: "IX_Vehiculos_Marca_id",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "Marca_id",
                table: "Vehiculos");

            migrationBuilder.AddColumn<string>(
                name: "Marca",
                table: "Vehiculos",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Marca",
                table: "Vehiculos");

            migrationBuilder.AddColumn<long>(
                name: "Marca_id",
                table: "Vehiculos",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_Marca_id",
                table: "Vehiculos",
                column: "Marca_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehiculos_Marcas_Marca_id",
                table: "Vehiculos",
                column: "Marca_id",
                principalTable: "Marcas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
