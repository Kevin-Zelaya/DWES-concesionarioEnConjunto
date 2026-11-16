

var builder = WebApplication.CreateBuilder(args);

//Documentacón Swagger
builder.Services.AddSwaggerGen();
//builder.Services.AddOpenApi();

builder.Services.AddAuthentication();

var app = builder.Build();

app.UseAuthentication();  // Primero: quien eres?
app.UseAuthorization();   // Segundo: que puedes hacer?

// Documentacón solo en Desarrollo
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger();
}

app.Run();
