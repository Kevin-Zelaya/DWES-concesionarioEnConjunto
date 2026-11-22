using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Models.Enums;
using APIAlquilerVehiculos.Validators.ValueGenerators;
using Microsoft.EntityFrameworkCore;

public static class CatalogoSeeder
{
    /// <summary>
    /// Inserta las marcas por defecto si todavía no existen.
    /// </summary>
    public static async Task SeedMarcasAsync(
        IServiceProvider services,
        ILogger logger)
    {
        var db = services.GetRequiredService<AppDbContext>();

        await EnsureMarcasExistAsync(db, logger);
    }

    /// <summary>
    /// Comprueba qué marcas existen e inserta las que faltan.
    /// </summary>
    private static async Task EnsureMarcasExistAsync(
        AppDbContext db,
        ILogger logger)
    {
        var marcasSeed = new List<Marca>
        {
            new()
            {
                Nombre = "Toyota",
                Wb = "https://www.toyota.es",
                Pais = "Japón",
                Activo = true
            },
            new()
            {
                Nombre = "BMW",
                Wb = "https://www.bmw.es",
                Pais = "Alemania",
                Activo = true
            },
            new()
            {
                Nombre = "Mercedes-Benz",
                Wb = "https://www.mercedes-benz.es",
                Pais = "Alemania",
                Activo = true
            },
            new()
            {
                Nombre = "Volkswagen",
                Wb = "https://www.volkswagen.es",
                Pais = "Alemania",
                Activo = true
            },
            new()
            {
                Nombre = "Audi",
                Wb = "https://www.audi.es",
                Pais = "Alemania",
                Activo = true
            },
            new()
            {
                Nombre = "Ford",
                Wb = "https://www.ford.es",
                Pais = "Estados Unidos",
                Activo = true
            },
            new()
            {
                Nombre = "Renault",
                Wb = "https://www.renault.es",
                Pais = "Francia",
                Activo = true
            },
            new()
            {
                Nombre = "Peugeot",
                Wb = "https://www.peugeot.es",
                Pais = "Francia",
                Activo = true
            },
            new()
            {
                Nombre = "Hyundai",
                Wb = "https://www.hyundai.com/es",
                Pais = "Corea del Sur",
                Activo = true
            },
            new()
            {
                Nombre = "Kia",
                Wb = "https://www.kia.com/es",
                Pais = "Corea del Sur",
                Activo = true
            },
            new()
            {
                Nombre = "SEAT",
                Wb = "https://www.seat.es",
                Pais = "España",
                Activo = true
            },
            new()
            {
                Nombre = "Škoda",
                Wb = "https://www.skoda.es",
                Pais = "República Checa",
                Activo = true
            },
            new()
            {
                Nombre = "Volvo",
                Wb = "https://www.volvocars.com/es",
                Pais = "Suecia",
                Activo = true
            },
            new()
            {
                Nombre = "Nissan",
                Wb = "https://www.nissan.es",
                Pais = "Japón",
                Activo = true
            },
            new()
            {
                Nombre = "Honda",
                Wb = "https://www.honda.es",
                Pais = "Japón",
                Activo = true
            },
            new()
            {
                Nombre = "Jeep",
                Wb = "https://www.jeep.es",
                Pais = "Estados Unidos",
                Activo = true
            },
            new()
            {
                Nombre = "MINI",
                Wb = "https://www.mini.es",
                Pais = "Reino Unido",
                Activo = true
            }
        };

        var nombresExistentes = await db.Marcas
            .Select(m => m.Nombre)
            .ToListAsync();

        var nombres = nombresExistentes
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var marcasNuevas = marcasSeed
            .Where(m => !nombres.Contains(m.Nombre))
            .ToList();

        if (marcasNuevas.Count == 0)
        {
            logger.LogInformation("Todas las marcas seed ya existen.");
            return;
        }

        var ahora = DateTime.UtcNow;

        marcasNuevas.ForEach(m => m.CreatedAt = ahora);

        await db.Marcas.AddRangeAsync(marcasNuevas);
        await db.SaveChangesAsync();

        logger.LogInformation(
            "Se han creado {Cantidad} marcas seed.",
            marcasNuevas.Count);
    }

    /// <summary>
    /// Inserta los vehículos por defecto si todavía no existen.
    /// </summary>
    public static async Task SeedVehiculosAsync(
        IServiceProvider services,
        ILogger logger,
        long concesionarioId)
    {
        var db = services.GetRequiredService<AppDbContext>();

        await EnsureVehiculosExistAsync(db, logger, concesionarioId);
    }

    /// <summary>
    /// Comprueba qué vehículos existen e inserta los que faltan.
    /// </summary>
    private static async Task EnsureVehiculosExistAsync(
        AppDbContext db,
        ILogger logger,
        long concesionarioId)
    {
        var marcas = await db.Marcas
            .ToDictionaryAsync(m => m.Nombre, m => m.Id);

        var matriculasExistentes = await db.Vehiculos
            .Select(v => v.Matricula)
            .ToListAsync();

        var matriculas = matriculasExistentes
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var ahora = DateTime.UtcNow;

        var vehiculosSeed = new List<(string Marca, Vehiculo Vehiculo)>
        {
            (
                "Toyota",
                new Vehiculo
                {
                    Matricula = "1234BCD",
                    Modelo = "Corolla",
                    Acriss = "ILAH",
                    categoria = CategoriaEnum.Sedan,
                    precioPorDia = 45m,
                    Color = "Blanco",
                    Combustible = CombustibleEnum.Hibrido,
                    Maletero = 470,
                    NumeroPlazas = 5,
                    Autonomia = 800,
                    Imagenes = 0,
                    Estado = EstadoEnum.Disponible,
                    UltimaRevision = ahora.AddMonths(-2),
                    Activo = true
                }
            ),
            (
                "Renault",
                new Vehiculo
                {
                    Matricula = "2345BCD",
                    Modelo = "Megane",
                    Acriss = "CDMD",
                    categoria = CategoriaEnum.Hatchback,
                    precioPorDia = 35m,
                    Color = "Gris",
                    Combustible = CombustibleEnum.Diesel,
                    Maletero = 384,
                    NumeroPlazas = 5,
                    Autonomia = 900,
                    Imagenes = 0,
                    Estado = EstadoEnum.Disponible,
                    UltimaRevision = ahora.AddMonths(-3),
                    Activo = true
                }
            ),
            (
                "Nissan",
                new Vehiculo
                {
                    Matricula = "3456BCD",
                    Modelo = "Ariya",
                    Acriss = "IFAE",
                    categoria = CategoriaEnum.Suv,
                    precioPorDia = 65m,
                    Color = "Negro",
                    Combustible = CombustibleEnum.Electrico,
                    Maletero = 466,
                    NumeroPlazas = 5,
                    Autonomia = 500,
                    Imagenes = 0,
                    Estado = EstadoEnum.Disponible,
                    UltimaRevision = ahora.AddMonths(-1),
                    Activo = true
                }
            ),
            (
                "Jeep",
                new Vehiculo
                {
                    Matricula = "4567BCD",
                    Modelo = "Wrangler",
                    Acriss = "FJBD",
                    categoria = CategoriaEnum.Todoterreno,
                    precioPorDia = 85m,
                    Color = "Verde",
                    Combustible = CombustibleEnum.Diesel,
                    Maletero = 533,
                    NumeroPlazas = 5,
                    Autonomia = 750,
                    Imagenes = 0,
                    Estado = EstadoEnum.Disponible,
                    UltimaRevision = ahora.AddMonths(-2),
                    Activo = true
                }
            ),
            (
                "Volkswagen",
                new Vehiculo
                {
                    Matricula = "5678BCD",
                    Modelo = "Passat Variant",
                    Acriss = "IWMD",
                    categoria = CategoriaEnum.Familiar,
                    precioPorDia = 55m,
                    Color = "Azul",
                    Combustible = CombustibleEnum.Diesel,
                    Maletero = 650,
                    NumeroPlazas = 5,
                    Autonomia = 1000,
                    Imagenes = 0,
                    Estado = EstadoEnum.Disponible,
                    UltimaRevision = ahora.AddMonths(-4),
                    Activo = true
                }
            ),
            (
                "BMW",
                new Vehiculo
                {
                    Matricula = "6789BCD",
                    Modelo = "Serie 4",
                    Acriss = "PEAV",
                    categoria = CategoriaEnum.Coupe,
                    precioPorDia = 90m,
                    Color = "Negro",
                    Combustible = CombustibleEnum.Gasolina,
                    Maletero = 440,
                    NumeroPlazas = 4,
                    Autonomia = 700,
                    Imagenes = 0,
                    Estado = EstadoEnum.Disponible,
                    UltimaRevision = ahora.AddMonths(-2),
                    Activo = true
                }
            ),
            (
                "MINI",
                new Vehiculo
                {
                    Matricula = "7890BCD",
                    Modelo = "Cabrio",
                    Acriss = "MTAV",
                    categoria = CategoriaEnum.Descapotable,
                    precioPorDia = 75m,
                    Color = "Rojo",
                    Combustible = CombustibleEnum.Gasolina,
                    Maletero = 215,
                    NumeroPlazas = 4,
                    Autonomia = 650,
                    Imagenes = 0,
                    Estado = EstadoEnum.Disponible,
                    UltimaRevision = ahora.AddMonths(-3),
                    Activo = true
                }
            ),
            (
                "Renault",
                new Vehiculo
                {
                    Matricula = "8901BCD",
                    Modelo = "Grand Scenic",
                    Acriss = "IMMD",
                    categoria = CategoriaEnum.Monovolumen,
                    precioPorDia = 60m,
                    Color = "Gris",
                    Combustible = CombustibleEnum.Diesel,
                    Maletero = 718,
                    NumeroPlazas = 7,
                    Autonomia = 950,
                    Imagenes = 0,
                    Estado = EstadoEnum.Disponible,
                    UltimaRevision = ahora.AddMonths(-1),
                    Activo = true
                }
            ),
            (
                "Ford",
                new Vehiculo
                {
                    Matricula = "9012BCD",
                    Modelo = "Transit",
                    Acriss = "FKMD",
                    categoria = CategoriaEnum.Furgoneta,
                    precioPorDia = 70m,
                    Color = "Blanco",
                    Combustible = CombustibleEnum.Diesel,
                    Maletero = 3000,
                    NumeroPlazas = 3,
                    Autonomia = 850,
                    Imagenes = 0,
                    Estado = EstadoEnum.Disponible,
                    UltimaRevision = ahora.AddMonths(-2),
                    Activo = true
                }
            )
        };

        var vehiculosNuevos = new List<Vehiculo>();

        foreach (var (nombreMarca, vehiculo) in vehiculosSeed)
        {
            if (matriculas.Contains(vehiculo.Matricula))
                continue;

            if (!marcas.TryGetValue(nombreMarca, out var marcaId))
            {
                logger.LogWarning(
                    "No se ha encontrado la marca {Marca}. No se insertará el vehículo {Modelo}.",
                    nombreMarca,
                    vehiculo.Modelo);

                continue;
            }

            vehiculo.Concesionario_id = concesionarioId;
            vehiculo.CreatedAt = ahora;

            vehiculosNuevos.Add(vehiculo);
        }

        if (vehiculosNuevos.Count == 0)
        {
            logger.LogInformation("Todos los vehículos seed ya existen.");
            return;
        }
        // foreach (var vehiculo in vehiculosNuevos)
        // {   
        //     vehiculo.Id = YoutubeIdValueGenerator.GenerateId();

        //     logger.LogInformation(
        //         "Vehículo {Modelo}: ID generado {Id}",
        //         vehiculo.Modelo,
        //         vehiculo.Id);
        // }

        await db.Vehiculos.AddRangeAsync(vehiculosNuevos);
        await db.SaveChangesAsync();

        logger.LogInformation(
            "Se han creado {Cantidad} vehículos seed.",
            vehiculosNuevos.Count);
    }

    /// <summary>
    /// Inserta las pólizas por defecto si todavía no existen.
    /// </summary>
    public static async Task SeedPolizasAsync(
        IServiceProvider services,
        ILogger logger)
    {
        var db = services.GetRequiredService<AppDbContext>();

        await EnsurePolizasExistAsync(db, logger);
    }

    /// <summary>
    /// Comprueba qué pólizas existen e inserta las que faltan.
    /// </summary>
    private static async Task EnsurePolizasExistAsync(
        AppDbContext db,
        ILogger logger)
    {
        var polizasSeed = new List<Poliza>
        {
            new()
            {
                Nombre = "Básica",
                Descripcion = "Póliza básica obligatoria incluida en todas las reservas.",
                IncrementoDiario = 0m,
                Activa = true
            },
            new()
            {
                Nombre = "Terceros ampliado",
                Descripcion = "Cobertura ampliada frente a daños a terceros, robo e incendio.",
                IncrementoDiario = 5m,
                Activa = true
            },
            new()
            {
                Nombre = "Franquicia reducida",
                Descripcion = "Cobertura que reduce el importe de la franquicia en caso de siniestro.",
                IncrementoDiario = 10m,
                Activa = true
            },
            new()
            {
                Nombre = "Todo riesgo",
                Descripcion = "Cobertura ampliada que protege frente a daños propios del vehículo.",
                IncrementoDiario = 18m,
                Activa = true
            }
        };

        // Incluimos también las pólizas inactivas para evitar duplicados.
        var nombresExistentes = await db.Polizas
            .IgnoreQueryFilters()
            .Select(p => p.Nombre)
            .ToListAsync();

        var nombres = nombresExistentes
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var polizasNuevas = polizasSeed
            .Where(p => !nombres.Contains(p.Nombre))
            .ToList();

        if (polizasNuevas.Count == 0)
        {
            logger.LogInformation("Todas las pólizas seed ya existen.");
            return;
        }

        await db.Polizas.AddRangeAsync(polizasNuevas);
        await db.SaveChangesAsync();

        logger.LogInformation(
            "Se han creado {Cantidad} pólizas seed.",
            polizasNuevas.Count);
    }
}