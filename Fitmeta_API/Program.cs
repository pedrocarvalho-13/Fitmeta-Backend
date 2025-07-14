using Fitmeta_API.Data;
using Fitmeta_API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models; // Adicionado para o uso de OpenApi/Swagger, se necessário
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(); // ESSENCIAL para que seus controladores (como AuthController) funcionem
builder.Services.AddEndpointsApiExplorer();

// Configurações do Swagger/OpenAPI
// Se você está usando AddControllers, o AddSwaggerGen é o mais comum.
// O AddOpenApi() é mais comum para Minimal APIs, mas pode coexistir se necessário.
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fitmeta API", Version = "v1" });
});
builder.Services.AddOpenApi(); // Mantenha se quiser a funcionalidade OpenApi separada

// Adiciona o AppDbContext e configura para usar PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registro dos serviços customizados
builder.Services.AddScoped<IUsuarioService, UsuarioService>(); // AddScoped significa que uma nova instância é criada por requisição

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]))
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fitmeta API v1");
        c.RoutePrefix = "swagger"; // Define que a UI do Swagger estará em /swagger
    });
}

// app.UseHttpsRedirection(); // Mantido comentado conforme sua preferência atual

// Middleware de roteamento
app.UseRouting(); // Importante para que o roteamento funcione corretamente

// Middleware de Autorização (ESSENCIAL para regras de segurança nos controladores)
app.UseAuthorization(); // LINHA 37 (agora na posição correta)

app.UseAuthentication();

// Middleware para mapear os controladores (ESSENCIAL para que AuthController funcione)
app.MapControllers();   // LINHA 38 (agora na posição correta)


// Configuração do OpenAPI se você estiver usando MapOpenApi
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Mantenha se quiser a funcionalidade OpenApi separada
}

// Seu endpoint WeatherForecast de exemplo (pode ser removido depois)
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