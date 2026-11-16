var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configuración del servicio de almacenamiento de archivos
builder.Services.AddStorage();

// Configuración de SignalR para notificaciones en tiempo real
builder.Services.AddSignalRconfiguration();

var app = builder.Build();

// Inicialización y preparación del directorio de subidas (uploads)
app.InitializeStorage(app.Environment.IsDevelopment());

// Habilita el suministro de archivos estáticos para acceder a las imágenes guardadas
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Mapeo del endpoint del Hub de SignalR para las notificaciones
app.MapHub<CocheHubs>("/hubs/coche");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}