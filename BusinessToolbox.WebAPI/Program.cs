using Amazon.Lambda.AspNetCoreServer.Hosting;
using System.Text;
using BusinessToolbox.Application.Interfaces;
using BusinessToolbox.Application.Services;
using BusinessToolbox.Infrastructure.Persistence;
using BusinessToolbox.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// 1. Database Configuration (PostgreSQL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Dependency Injection
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddHttpClient();

// 3. Authentication & Authorization (Supabase JWT)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Supabase:Issuer"];
        options.Audience = builder.Configuration["Supabase:Audience"];
        options.RequireHttpsMetadata = false; // Solo para desarrollo si no hay HTTPS local sólido

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Supabase:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Supabase:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddAuthorization();

// 5. CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://business-toolbox.yosoyalexisromero.site"
              )
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 4. API Controllers & Documentation
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Business Toolbox API", Version = "v1" });
    
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduce tu token JWT de Supabase"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

// Configuración para AWS Lambda (Solo se activa si corre en AWS)
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "openapi/{documentName}.json";
    });
    app.MapScalarApiReference(options => 
    {
        options.WithTitle("Business Toolbox API")
               .WithTheme(ScalarTheme.Moon)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Content(@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Business Toolbox API</title>
    <style>
        body { font-family: 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background: #0f172a; color: #f8fafc; display: flex; justify-content: center; align-items: center; height: 100vh; margin: 0; }
        .container { text-align: center; background: #1e293b; padding: 3rem; border-radius: 1rem; box-shadow: 0 10px 15px -3px rgb(0 0 0 / 0.5); max-width: 500px; }
        h1 { color: #38bdf8; margin-bottom: 0.5rem; }
        p { color: #94a3b8; margin-bottom: 2rem; line-height: 1.5; }
        .btn { background: #38bdf8; color: #0f172a; padding: 0.75rem 1.5rem; border-radius: 0.5rem; text-decoration: none; font-weight: bold; transition: background 0.2s; display: inline-block; }
        .btn:hover { background: #7dd3fc; }
        .status { margin-top: 2rem; font-size: 0.875rem; color: #10b981; display: flex; align-items: center; justify-content: center; gap: 0.5rem; }
        .dot { height: 10px; width: 10px; background-color: #10b981; border-radius: 50%; display: inline-block; animation: pulse 2s infinite; }
        @keyframes pulse { 0% { box-shadow: 0 0 0 0 rgba(16, 185, 129, 0.7); } 70% { box-shadow: 0 0 0 10px rgba(16, 185, 129, 0); } 100% { box-shadow: 0 0 0 0 rgba(16, 185, 129, 0); } }
    </style>
</head>
<body>
    <div class='container'>
        <h1>Business Toolbox API</h1>
        <p>Solo para personal autorizado del proyecto. La API está lista para recibir peticiones y procesar la lógica de negocio.</p>
        <a href='/scalar/v1' class='btn'>Ver Documentación (API Reference)</a>
        <div class='status'>
            <span class='dot'></span> Backend Activo y en Línea
        </div>
    </div>
</body>
</html>
", "text/html"));

app.Run();
