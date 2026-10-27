using Haven.Application.Abstractions;
using Haven.Application.Listings;
using Haven.Infrastructure.Context;
using Haven.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// OpenAPI
builder.Services.AddOpenApi();

// Database
var connectionString =
    builder.Configuration.GetConnectionString("HavenDb");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'HavenDb' was not configured.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Listing services
builder.Services.AddScoped<IListingRepository, EfListingRepository>();
builder.Services.AddScoped<IPropertyTypeRepository, EfPropertyTypeRepository>();
builder.Services.AddScoped<ListingApplicationService>();
builder.Services.AddSingleton(TimeProvider.System);

var app = builder.Build();

// Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middleware
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// Static assets
app.MapStaticAssets();

// OpenAPI and Scalar only in Development
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options.WithTitle("HAVEN API");
    });
}

// MVC routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllers();

app.Run();

// Expose the entry point for WebApplicationFactory in integration tests.
public partial class Program { }
