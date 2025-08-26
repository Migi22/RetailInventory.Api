using Microsoft.EntityFrameworkCore;
using RetailInventory.Api.Data;
using RetailInventory.Api.Models;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
{
    
}

var builder = WebApplication.CreateBuilder(args);

// 1) Register services (DI container)
builder.Services.AddControllers();              // MVC controllers (attribute-routed APIs)
builder.Services.AddEndpointsApiExplorer();     // Exposes endpoints for Swagger
builder.Services.AddSwaggerGen();               // Generates OpenAPI/Swagger doc

// 2) EF Core: wire DbContext to SQL Server (enable retry for transient errors)
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(conn, sql => sql.EnableRetryOnFailure()));

// 2.5) JWT Authentication
// --- JWT Authentication Configuration ---
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException(
        "JWT Key is not configured. Please set 'Jwt:Key' in appsettings.json or in user secrets."
    );
}

var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// 3) Dev-time helpers (Swagger) + DB migrations
if (app.Environment.IsDevelopment())
{
    // Apply EF Core migrations automatically in Development
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Reset DB each run in dev mode
    db.Database.EnsureDeleted(); // drop existing database
    db.Database.EnsureCreated(); // create new database

    // Call SeedData
    AppDbContext.SeedData(db);

    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // In non-development (e.g., Docker container), apply migrations on startup
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var retries = 10;
    while (retries > 0)
    {
        try
        {
            db.Database.Migrate(); // or EnsureCreated() if you want
            AppDbContext.SeedData(db); // optional: seed data
            break;
        }
        catch (Exception ex)
        {
            retries--;
            Console.WriteLine($"DB not ready yet. Retrying in 5s... ({retries} retries left)");
            Thread.Sleep(5000); // wait 5 seconds
        }
    }
}

// 4) Middleware pipeline
// Avoid HTTPS redirection in container unless HTTPS is configured
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// JWT Authentication middleware
app.UseAuthentication();

// Keep Authorization middleware
app.UseAuthorization();

// 5) Map controller endpoints (attribute routes)
app.MapGet("/health", () => Results.Ok("OK"));
app.MapControllers();

app.Run();
