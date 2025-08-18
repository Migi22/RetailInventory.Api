using Microsoft.EntityFrameworkCore;
using RetailInventory.Api.Data;
using RetailInventory.Api.Models;
using BCrypt.Net;

var builder = WebApplication.CreateBuilder(args);

// 1) Register services (DI container)
builder.Services.AddControllers();              // MVC controllers (attribute-routed APIs)
builder.Services.AddEndpointsApiExplorer();     // Exposes endpoints for Swagger
builder.Services.AddSwaggerGen();               // Generates OpenAPI/Swagger doc

// 2) EF Core: wire DbContext to SQL Server
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(conn));

var app = builder.Build();

// 3) Dev-time helpers (Swagger) + DB migrations
if (app.Environment.IsDevelopment())
{
    // Apply EF Core migrations automatically in Development
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // Call SeedData
    AppDbContext.SeedData(db);

    app.UseSwagger();
    app.UseSwaggerUI();
}

// 4) Middleware pipeline
app.UseHttpsRedirection();

// Keep Authorization middleware
app.UseAuthorization();

// 5) Map controller endpoints (attribute routes)
app.MapControllers();

app.Run();
