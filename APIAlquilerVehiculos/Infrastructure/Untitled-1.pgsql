INSERT INTO "Reservas"
(
    "CodigoRefencia",
    "ClienteId",
    "TrabajadorId",
    "Categoria",
    "VehiculoId",
    "FechaCreacion",
    "FechaRecogida",
    "FechaDevolucion",
    "Estado",
    "PrecioTotal",
    "Notas"
)
VALUES
(
    'TEST01',
    1,
    1,
    'Descapotable',
    'MVs4n8PYCff',
    NOW(),
    '2026-11-23 10:00:00+00',
    '2026-11-28 10:00:00+00',
    'Confirmada',
    105.00,
    'Reserva de prueba para BackgroundService'
)
RETURNING *;