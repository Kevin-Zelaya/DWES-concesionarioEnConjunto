using APIAlquilerVehiculos.Entities;
using Microsoft.EntityFrameworkCore;

namespace APIAlquilerVehiculos.Tests.Infrastructure;

// Utilidad para tests con PostgreSQL: crea el esquema actual,
// repara la restricción GiST de solape en Reservas y limpia datos entre pruebas.
public static class TestMigrationHelper
{
    /// <summary>
    /// Crea las tablas del modelo actual y aplica la restricción de exclusión corregida en Reservas.
    /// </summary>
    public static async Task InitializeTestDatabaseAsync(
        AppDbContext context,
        CancellationToken cancellationToken = default)
    {
        // Genera todas las tablas/restricciones del modelo EF Core sin ejecutar migraciones antiguas con tstzrange
        await context.Database.EnsureCreatedAsync(cancellationToken);

        // Asegura que la restricción GiST de Reservas quede creada con tsrange y AT TIME ZONE 'UTC'
        await EnsureReservaExclusionConstraintAsync(context, cancellationToken);
    }

    /// <summary>
    /// Crea o recrea la restricción GiST EX_Reservas_Vehiculo_Fechas usando tsrange en UTC para evitar el error 42P17.
    /// </summary>
    public static async Task EnsureReservaExclusionConstraintAsync(
        AppDbContext context,
        CancellationToken cancellationToken = default)
    {
        // Script PL/pgSQL: comprueba si existe la tabla "Reservas", habilita btree_gist,
        // elimina cualquier versión previa de la restricción y crea la versión compatible con índice GiST.
        const string ensureSql = """
            DO $$
            BEGIN
                -- Solo actúa si la tabla "Reservas" ya existe en el esquema public
                IF to_regclass('public."Reservas"') IS NOT NULL THEN
                    -- Necesaria para indexar en GiST igualdad ("VehiculoId" =) junto con solape de rangos (&&)
                    CREATE EXTENSION IF NOT EXISTS btree_gist;

                    -- Borra la restricción anterior si existía para evitar duplicados o definiciones inválidas
                    ALTER TABLE "Reservas"
                        DROP CONSTRAINT IF EXISTS "EX_Reservas_Vehiculo_Fechas";

                    -- Crea la restricción de exclusión usando tsrange con fechas normalizadas a UTC (IMMUTABLE para GiST)
                    ALTER TABLE "Reservas"
                        ADD CONSTRAINT "EX_Reservas_Vehiculo_Fechas"
                        EXCLUDE USING gist (
                            "VehiculoId" WITH =,
                            tsrange(
                                ("FechaRecogida" AT TIME ZONE 'UTC'),
                                ("FechaDevolucion" AT TIME ZONE 'UTC'),
                                '[)'
                            ) WITH &&
                        )
                        -- Solo bloquea solapes en reservas que siguen activas
                        WHERE ("Estado" IN (
                            'Pendiente',
                            'Confirmada',
                            'EnCurso'
                        ));
                END IF;
            END
            $$;
            """;

        // Ejecuta el bloque SQL directamente contra la base de datos del contexto
        await context.Database.ExecuteSqlRawAsync(ensureSql, cancellationToken);
    }

    /// <summary>
    /// Vacía todas las tablas de datos del esquema public y reinicia secuencias de identidad sin borrar el esquema.
    /// </summary>
    public static async Task ResetDatabaseAsync(
        AppDbContext context,
        CancellationToken cancellationToken = default)
    {
        // Script PL/pgSQL: agrupa todas las tablas de usuario en "public" (salvo __EFMigrationsHistory)
        // y las trunca en cascada reiniciando contadores IDENTITY.
        const string truncateSql = """
            DO $$
            DECLARE
                -- Almacena la lista separada por comas de tablas a truncar
                tables_list text;
            BEGIN
                -- Busca tablas del esquema public excluyendo el historial de migraciones de EF Core
                SELECT string_agg(format('%I.%I', schemaname, tablename), ', ')
                INTO tables_list
                FROM pg_tables
                WHERE schemaname = 'public'
                  AND tablename <> '__EFMigrationsHistory';

                -- Si hay al menos una tabla, trunca todas de golpe, reinicia IDs y aplica CASCADE a dependientes
                IF tables_list IS NOT NULL AND length(tables_list) > 0 THEN
                    EXECUTE 'TRUNCATE TABLE ' || tables_list || ' RESTART IDENTITY CASCADE';
                END IF;
            END
            $$;
            """;

        // Lanza el limpiado en la base de datos de pruebas
        await context.Database.ExecuteSqlRawAsync(truncateSql, cancellationToken);
    }
}