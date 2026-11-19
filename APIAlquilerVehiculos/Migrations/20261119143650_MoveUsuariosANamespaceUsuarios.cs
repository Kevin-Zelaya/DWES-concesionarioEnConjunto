using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIAlquilerVehiculos.Migrations
{
    /// <inheritdoc />
    public partial class MoveUsuariosANamespaceUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehiculos_Marca_Marca_id",
                table: "Vehiculos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Marca",
                table: "Marca");

            migrationBuilder.RenameTable(
                name: "Marca",
                newName: "Marcas");

            migrationBuilder.RenameIndex(
                name: "IX_Marca_Nombre",
                table: "Marcas",
                newName: "IX_Marcas_Nombre");

            migrationBuilder.AddColumn<string>(
                name: "Pais",
                table: "Marcas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Marcas",
                table: "Marcas",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehiculos_Marcas_Marca_id",
                table: "Vehiculos",
                column: "Marca_id",
                principalTable: "Marcas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehiculos_Marcas_Marca_id",
                table: "Vehiculos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Marcas",
                table: "Marcas");

            migrationBuilder.DropColumn(
                name: "Pais",
                table: "Marcas");

            migrationBuilder.RenameTable(
                name: "Marcas",
                newName: "Marca");

            migrationBuilder.RenameIndex(
                name: "IX_Marcas_Nombre",
                table: "Marca",
                newName: "IX_Marca_Nombre");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Marca",
                table: "Marca",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehiculos_Marca_Marca_id",
                table: "Vehiculos",
                column: "Marca_id",
                principalTable: "Marca",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
