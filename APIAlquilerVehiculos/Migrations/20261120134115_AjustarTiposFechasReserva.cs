
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIAlquilerVehiculos.Migrations
{
    public partial class AjustarTiposFechasReserva : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Eliminar la restricción anterior
            migrationBuilder.Sql("""
                ALTER TABLE "Reservas"
                DROP CONSTRAINT IF EXISTS "EX_Reservas_Vehiculo_Fechas";
            """);

            // Cambiar los tipos de las fechas
            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaRecogida",
                table: "Reservas",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaDevolucion",
                table: "Reservas",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            // Crear la restricción con tsrange
            migrationBuilder.Sql("""
                ALTER TABLE "Reservas"
                ADD CONSTRAINT "EX_Reservas_Vehiculo_Fechas"
                EXCLUDE USING gist (
                    "VehiculoId" WITH =,
                    tsrange(
                        "FechaRecogida",
                        "FechaDevolucion",
                        '[)'
                    ) WITH &&
                )
                WHERE ("Estado" IN (
                    'Pendiente',
                    'Confirmada',
                    'EnCurso'
                ));
            """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Eliminar la restricción actual
            migrationBuilder.Sql("""
                ALTER TABLE "Reservas"
                DROP CONSTRAINT IF EXISTS "EX_Reservas_Vehiculo_Fechas";
            """);

            // Restaurar los tipos anteriores
            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaRecogida",
                table: "Reservas",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaDevolucion",
                table: "Reservas",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            // Restaurar la restricción original
            migrationBuilder.Sql("""
                ALTER TABLE "Reservas"
                ADD CONSTRAINT "EX_Reservas_Vehiculo_Fechas"
                EXCLUDE USING gist (
                    "VehiculoId" WITH =,
                    tstzrange(
                        "FechaRecogida",
                        "FechaDevolucion",
                        '[)'
                    ) WITH &&
                )
                WHERE ("Estado" IN (
                    'Pendiente',
                    'Confirmada',
                    'EnCurso'
                ));
            """);
        }
    }
}
