

var builder = WebApplication.CreateBuilder(args);


//builder.Services.AddOpenApi();

builder.Services.AddAuthentication();

var app = builder.Build();

app.UseAuthentication();  // Primero: quien eres?
app.UseAuthorization();   // Segundo: que puedes hacer?

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();
