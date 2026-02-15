using ECommerce.API.Common.Auth.Filters;
using ECommerce.API.Common.Data;
using ECommerce.API.Common.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Base de datos ──────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(maxRetryCount: 3)
    )
);

// ── JWT ────────────────────────────────────────────────────────────────────────
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret es obligatorio en appsettings.json");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ── Servicios comunes ──────────────────────────────────────────────────────────
// Estos son los únicos servicios "globales" que registramos acá.
// En Vertical Slice la lógica de negocio vive dentro de cada feature,
// no en servicios centralizados. Lo que registramos acá son utilidades
// que múltiples features genuinamente comparten.
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<AuthorizationFilter>();

// ── Controllers ────────────────────────────────────────────────────────────────
builder.Services.AddControllers(options =>
{
    options.Filters.AddService<AuthorizationFilter>();
});

// ── OpenAPI + Scalar ───────────────────────────────────────────────────────────
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Components ??= new OpenApiComponents();

        if (document.Components.SecuritySchemes == null)
            document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Ingresá tu JWT token"
        };

        return Task.CompletedTask;
    });
});

// ── CORS ───────────────────────────────────────────────────────────────────────
var frontendUrl = builder.Configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(frontendUrl)
              .AllowAnyHeader()
              .AllowAnyMethod()
    );
});

// ══════════════════════════════════════════════════════════════════════════════
// Pipeline HTTP
// ══════════════════════════════════════════════════════════════════════════════
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("ECommerce API")
               .WithTheme(ScalarTheme.Purple);
    });

    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => new
{
    status = "OK",
    service = "ECommerce API",
    env = app.Environment.EnvironmentName
});

app.Run();
