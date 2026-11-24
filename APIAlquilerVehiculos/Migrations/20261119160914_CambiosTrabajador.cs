using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIAlquilerVehiculos.Migrations
{
    /// <inheritdoc />
    public partial class CambiosTrabajador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cliente_usuarios_IdUsuario",
                table: "Cliente");

            migrationBuilder.DropForeignKey(
                name: "FK_Oficinas_Trabajador_IdResponsable",
                table: "Oficinas");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_Cliente_ClienteId",
                table: "Reservas");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_Trabajador_TrabajadorId",
                table: "Reservas");

            migrationBuilder.DropForeignKey(
                name: "FK_Trabajador_usuarios_IdUsuario",
                table: "Trabajador");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Trabajador",
                table: "Trabajador");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cliente",
                table: "Cliente");

            migrationBuilder.RenameTable(
                name: "Trabajador",
                newName: "Trabajadores");

            migrationBuilder.RenameTable(
                name: "Cliente",
                newName: "Clientes");

            migrationBuilder.RenameIndex(
                name: "IX_Trabajador_IdUsuario",
                table: "Trabajadores",
                newName: "IX_Trabajadores_IdUsuario");

            migrationBuilder.RenameIndex(
                name: "IX_Cliente_Pasaporte",
                table: "Clientes",
                newName: "IX_Clientes_Pasaporte");

            migrationBuilder.RenameIndex(
                name: "IX_Cliente_Nie",
                table: "Clientes",
                newName: "IX_Clientes_Nie");

            migrationBuilder.RenameIndex(
                name: "IX_Cliente_IdUsuario",
                table: "Clientes",
                newName: "IX_Clientes_IdUsuario");

            migrationBuilder.RenameIndex(
                name: "IX_Cliente_Dni",
                table: "Clientes",
                newName: "IX_Clientes_Dni");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Trabajadores",
                table: "Trabajadores",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Clientes",
                table: "Clientes",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Trabajadores_Dni",
                table: "Trabajadores",
                column: "Dni",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_usuarios_IdUsuario",
                table: "Clientes",
                column: "IdUsuario",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Oficinas_Trabajadores_IdResponsable",
                table: "Oficinas",
                column: "IdResponsable",
                principalTable: "Trabajadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_Clientes_ClienteId",
                table: "Reservas",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_Trabajadores_TrabajadorId",
                table: "Reservas",
                column: "TrabajadorId",
                principalTable: "Trabajadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trabajadores_usuarios_IdUsuario",
                table: "Trabajadores",
                column: "IdUsuario",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_usuarios_IdUsuario",
                table: "Clientes");

            migrationBuilder.DropForeignKey(
                name: "FK_Oficinas_Trabajadores_IdResponsable",
                table: "Oficinas");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_Clientes_ClienteId",
                table: "Reservas");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_Trabajadores_TrabajadorId",
                table: "Reservas");

            migrationBuilder.DropForeignKey(
                name: "FK_Trabajadores_usuarios_IdUsuario",
                table: "Trabajadores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Trabajadores",
                table: "Trabajadores");

            migrationBuilder.DropIndex(
                name: "IX_Trabajadores_Dni",
                table: "Trabajadores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Clientes",
                table: "Clientes");

            migrationBuilder.RenameTable(
                name: "Trabajadores",
                newName: "Trabajador");

            migrationBuilder.RenameTable(
                name: "Clientes",
                newName: "Cliente");

            migrationBuilder.RenameIndex(
                name: "IX_Trabajadores_IdUsuario",
                table: "Trabajador",
                newName: "IX_Trabajador_IdUsuario");

            migrationBuilder.RenameIndex(
                name: "IX_Clientes_Pasaporte",
                table: "Cliente",
                newName: "IX_Cliente_Pasaporte");

            migrationBuilder.RenameIndex(
                name: "IX_Clientes_Nie",
                table: "Cliente",
                newName: "IX_Cliente_Nie");

            migrationBuilder.RenameIndex(
                name: "IX_Clientes_IdUsuario",
                table: "Cliente",
                newName: "IX_Cliente_IdUsuario");

            migrationBuilder.RenameIndex(
                name: "IX_Clientes_Dni",
                table: "Cliente",
                newName: "IX_Cliente_Dni");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Trabajador",
                table: "Trabajador",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cliente",
                table: "Cliente",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Cliente_usuarios_IdUsuario",
                table: "Cliente",
                column: "IdUsuario",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Oficinas_Trabajador_IdResponsable",
                table: "Oficinas",
                column: "IdResponsable",
                principalTable: "Trabajador",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_Cliente_ClienteId",
                table: "Reservas",
                column: "ClienteId",
                principalTable: "Cliente",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_Trabajador_TrabajadorId",
                table: "Reservas",
                column: "TrabajadorId",
                principalTable: "Trabajador",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trabajador_usuarios_IdUsuario",
                table: "Trabajador",
                column: "IdUsuario",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
