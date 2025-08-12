using Fitmeta_API.Data;
using Fitmeta_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Adiciona os serviços do controlador
builder.Services.AddControllers();

// Adiciona os serviços do Swagger/OpenAPI e configura para JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fitmeta API", Version = "v1" });

    // Configuração para exibir o botão de autorização JWT no Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder
            // --- ALTERADO AQUI ---
            .AllowAnyOrigin() // Permite qualquer origem
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Adiciona o AppDbContext e configura para usar PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registro dos serviços customizados
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// --- A VERSÃO CORRETA DA CONFIGURAÇÃO DO JWT É ESTA! ---
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

// Adiciona o serviço de autorização
builder.Services.AddAuthorization();

var app = builder.Build();

// Configura o pipeline de requisição HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// --- A ORDEM FOI AJUSTADA E UMA LINHA FOI ADICIONADA ---
app.UseRouting(); // Identifica para qual rota a requisição deve ir
app.UseCors("CorsPolicy"); // <-- LINHA ADICIONADA: Aplica a política de CORS
app.UseAuthentication(); // Lê o token JWT do cabeçalho
app.UseAuthorization(); // Verifica se o usuário autenticado pode acessar a rota
app.MapControllers(); // Executa o controlador correto

app.Run();

// using Fitmeta_API.Data;
// using Fitmeta_API.Services;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.OpenApi.Models; // Adicionado para o uso de OpenApi/Swagger, se necessário
// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.IdentityModel.Tokens;
// using System.Text;

// var builder = WebApplication.CreateBuilder(args);

// builder.Services.AddControllers();
// builder.Services.AddEndpointsApiExplorer();

// builder.Services.AddSwaggerGen(c =>
// {
//     c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fitmeta API", Version = "v1" });
// });
// builder.Services.AddOpenApi();


// builder.Services.AddDbContext<AppDbContext>(options =>
//     options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// // Registro dos serviços customizados
// builder.Services.AddScoped<IUsuarioService, UsuarioService>();

// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options =>
//     {
//         options.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidateIssuer = true,
//             ValidateAudience = true,
//             ValidateLifetime = true,
//             ValidateIssuerSigningKey = true,

//             ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
//             ValidAudience = builder.Configuration["JwtSettings:Audience"],
//             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]))
//         };
//     });

// builder.Services.AddScoped<IEmailService, EmailService>();

// builder.Services.AddAuthorization();

// builder.Services.AddControllers();
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

// var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI(c =>
//     {
//         c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fitmeta API v1");
//         c.RoutePrefix = "swagger";
//     });
// }

// app.UseRouting();

// app.UseAuthorization(); 

// app.UseAuthentication();

// app.MapControllers();


// if (app.Environment.IsDevelopment())
// {
//     app.MapOpenApi(); // Mantenha se quiser a funcionalidade OpenApi separada
// }

// app.Run();

// // record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
// // {
// //     public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
// // }