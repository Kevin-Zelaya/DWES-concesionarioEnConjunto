INSERT INTO "Reservas" (
    "ClienteId",
    "TrabajadorId",
    "Categoria",
    "VehiculoId",
    "FechaRecogida",
    "FechaDevolucion",
    "Estado",
    "PrecioTotal",
    "Notas"
)
SELECT
    (SELECT "Id" FROM "Clientes" LIMIT 1),
    (SELECT "Id" FROM "Trabajadores" LIMIT 1),
    v."categoria",
    v."Id",
    NOW() - INTERVAL '4 days',
    NOW() - INTERVAL '1 day',
    'EnCurso',
    150.00,
    'PRUEBA CU-24'
FROM "Vehiculos" v
WHERE v."Estado" = 'EnAlquiler'
  AND v."Activo" = TRUE
LIMIT 1;