using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIAlquilerVehiculos.Migrations
{
    /// <inheritdoc />
    public partial class TochaSolution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_Pago_PagoId",
                table: "Reservas");

            migrationBuilder.DropIndex(
                name: "IX_Reservas_PagoId",
                table: "Reservas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pago",
                table: "Pago");

            migrationBuilder.DropColumn(
                name: "Marca",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "PagoId",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "UltimosDigito",
                table: "Pago");

            migrationBuilder.RenameTable(
                name: "Pago",
                newName: "Pagos");

            migrationBuilder.AddColumn<long>(
                name: "MarcaId",
                table: "Vehiculos",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<string>(
                name: "TarjetaCifrada",
                table: "Pagos",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Importe",
                table: "Pagos",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AddColumn<DateTime>(
                name: "Fecha",
                table: "Pagos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ReferenciaPasarela",
                table: "Pagos",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "ReservaId",
                table: "Pagos",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "UltimosCuatroDigitos",
                table: "Pagos",
                type: "character varying(4)",
                maxLength: 4,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pagos",
                table: "Pagos",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_MarcaId",
                table: "Vehiculos",
                column: "MarcaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_ReferenciaPasarela",
                table: "Pagos",
                column: "ReferenciaPasarela",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_ReservaId",
                table: "Pagos",
                column: "ReservaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pagos_Reservas_ReservaId",
                table: "Pagos",
                column: "ReservaId",
                principalTable: "Reservas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehiculos_Marcas_MarcaId",
                table: "Vehiculos",
                column: "MarcaId",
                principalTable: "Marcas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pagos_Reservas_ReservaId",
                table: "Pagos");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehiculos_Marcas_MarcaId",
                table: "Vehiculos");

            migrationBuilder.DropIndex(
                name: "IX_Vehiculos_MarcaId",
                table: "Vehiculos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pagos",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_ReferenciaPasarela",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_ReservaId",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "MarcaId",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "Fecha",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "ReferenciaPasarela",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "ReservaId",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "UltimosCuatroDigitos",
                table: "Pagos");

            migrationBuilder.RenameTable(
                name: "Pagos",
                newName: "Pago");

            migrationBuilder.AddColumn<string>(
                name: "Marca",
                table: "Vehiculos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "PagoId",
                table: "Reservas",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TarjetaCifrada",
                table: "Pago",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<double>(
                name: "Importe",
                table: "Pago",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AddColumn<string>(
                name: "UltimosDigito",
                table: "Pago",
                type: "character varying(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pago",
                table: "Pago",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_PagoId",
                table: "Reservas",
                column: "PagoId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_Pago_PagoId",
                table: "Reservas",
                column: "PagoId",
                principalTable: "Pago",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
