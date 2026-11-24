using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIAlquilerVehiculos.Migrations
{
    /// <inheritdoc />
    public partial class ChangeReservaDatesToTimestampTz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Eliminar la restricción anterior.
            migrationBuilder.Sql("""
                ALTER TABLE "Reservas"
                DROP CONSTRAINT "EX_Reservas_Vehiculo_Fechas";
                """);

            // Cambiar FechaRecogida.
            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaRecogida",
                table: "Reservas",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            // Cambiar FechaDevolucion.
            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaDevolucion",
                table: "Reservas",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            // Crear la restricción compatible con timestamptz.
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Reservas"
                DROP CONSTRAINT "EX_Reservas_Vehiculo_Fechas";
                """);

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
    }
}
