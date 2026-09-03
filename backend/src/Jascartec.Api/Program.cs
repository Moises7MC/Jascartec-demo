using System.Text;
using Jascartec.Api.Middleware;
using Jascartec.Application;
using Jascartec.Application.Common;
using Jascartec.Infrastructure;
using Jascartec.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ===================== Servicios =====================
builder.Services.AddControllers();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Falta la sección 'Jwt' en la configuración.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });
builder.Services.AddAuthorization();

// CORS abierto: el frontend hoy es un HTML estático servido por file:// o un origen
// dinámico durante desarrollo — mismo enfoque ya usado con el proxy de DNI en Cloudflare.
const string CorsPolicy = "FrontendPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Jascartec API", Version = "v1" });
    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pegue solo el token JWT (sin la palabra 'Bearer')."
    };
    options.AddSecurityDefinition("Bearer", jwtScheme);
});

var app = builder.Build();

// ===================== Base de datos: migrar + sembrar =====================
// La migración del esquema siempre se aplica (hace falta en cualquier instalación).
// Los datos de EJEMPLO (marcas/productos/clientes/ventas de prueba) solo se cargan
// si Seed:CargarDatosDemo está en true — en la instalación real del negocio se deja
// en false ("appsettings.json") para que el sistema empiece completamente en cero;
// el usuario esencial ("admin") sí se crea siempre, para poder entrar la primera vez.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<JascartecDbContext>();
    await db.Database.MigrateAsync();
    await DataSeeder.SeedEssentialsAsync(db);

    if (builder.Configuration.GetValue("Seed:CargarDatosDemo", app.Environment.IsDevelopment()))
        await DataSeeder.SeedDemoDataAsync(db);
}

// ===================== Middleware pipeline =====================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
