using Haven.Application.Abstractions;
using Haven.Application.Consultations;
using Haven.Application.Dashboard;
using Haven.Application.Favorites;
using Haven.Application.Listings;
using Haven.Application.Locations;
using Haven.Application.PropertyTypes;
using Haven.Domain.Privacy;
using Haven.Application.Tags;
using Haven.Infrastructure.Context;
using Haven.Infrastructure.Identity;
using Haven.Infrastructure.Repositories;
using Haven.Infrastructure.Storage;
using Haven.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddLocalization();
builder.Services.AddControllersWithViews(options =>
    {
        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
        options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(name => $"Trường {name} là bắt buộc.");
        options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((_, _) => "Giá trị đã nhập chưa hợp lệ.");
        options.ModelBindingMessageProvider.SetUnknownValueIsInvalidAccessor(name => $"Giá trị của trường {name} chưa hợp lệ.");
        options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(name => $"Trường {name} phải là số.");
    })
    .AddDataAnnotationsLocalization(options =>
        options.DataAnnotationLocalizerProvider = (_, factory) => factory.Create(typeof(Haven.Web.Services.SharedResource)));

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var factory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var details = factory.CreateValidationProblemDetails(
            context.HttpContext,
            context.ModelState,
            statusCode: StatusCodes.Status400BadRequest);
        details.Title = "Thông tin gửi lên chưa hợp lệ.";
        return new BadRequestObjectResult(details);
    };
});

// Authentication and authorization
builder.Services
    .AddIdentity<HavenUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddErrorDescriber<Haven.Web.Services.VietnameseIdentityErrorDescriber>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "Haven.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.LoginPath = "/account/login";
    options.AccessDeniedPath = "/account/access-denied";
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

// Revoke existing sign-in cookies shortly after an account lockout or a
// security-stamp change. Identity's validator uses this standard mechanism.
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
    options.ValidationInterval = TimeSpan.FromMinutes(1));

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
    options.Cookie.Name = "Haven.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var vietnamese = CultureInfo.GetCultureInfo("vi-VN");
    options.DefaultRequestCulture = new RequestCulture(vietnamese);
    options.SupportedCultures = [vietnamese];
    options.SupportedUICultures = [vietnamese];
});

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
builder.Services.AddScoped<ITagRepository, EfTagRepository>();
builder.Services.AddScoped<IAdminAnalyticsRepository, EfAdminAnalyticsRepository>();
builder.Services.AddScoped<IFavoriteRepository, EfFavoriteRepository>();
builder.Services.AddScoped<IConsultationRepository, EfConsultationRepository>();
builder.Services.AddScoped<IListingImageRepository, EfListingImageRepository>();
builder.Services.AddScoped<IListingVideoRepository, EfListingVideoRepository>();
builder.Services.AddScoped<IAdministrativeLocationRepository, EfAdministrativeLocationRepository>();
builder.Services.AddSingleton(services => new LocalListingImageStorage(
    services.GetRequiredService<IWebHostEnvironment>().ContentRootPath,
    builder.Configuration["Haven:Media:StoragePath"]
        ?? builder.Configuration["Haven:ListingImages:StoragePath"]));
builder.Services.AddSingleton<IListingImageStorage>(services => services.GetRequiredService<LocalListingImageStorage>());
builder.Services.AddSingleton<IListingVideoStorage>(services => services.GetRequiredService<LocalListingImageStorage>());
builder.Services.AddSingleton(new ListingVideoOptions
{
    MaximumVideosPerListing = builder.Configuration.GetValue<int?>("Haven:ListingVideos:MaximumVideosPerListing") ?? 3,
    MaximumFileLengthBytes = builder.Configuration.GetValue<long?>("Haven:ListingVideos:MaximumFileLengthBytes") ?? 100 * 1024 * 1024
});
builder.Services.AddScoped<ListingApplicationService>();
builder.Services.AddScoped<ListingImageApplicationService>();
builder.Services.AddScoped<ListingVideoApplicationService>();
builder.Services.AddScoped<AdministrativeLocationApplicationService>();
builder.Services.AddSingleton<AvatarImageStorage>();
builder.Services.AddScoped<FavoriteApplicationService>();
builder.Services.AddScoped<ConsultationApplicationService>();
builder.Services.AddScoped<AdminDashboardApplicationService>();
builder.Services.AddScoped<PropertyTypeApplicationService>();
builder.Services.AddScoped<TagApplicationService>();
builder.Services.AddSingleton(TimeProvider.System);

var app = builder.Build();

await IdentityBootstrapper.ProvisionConfiguredAdminAsync(
    app.Services,
    builder.Configuration);
await AdministrativeLocationSeeder.SeedAsync(app.Services);

// Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middleware
app.UseHttpsRedirection();
app.UseRouting();
app.UseRequestLocalization();
app.UseAuthentication();
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
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllers();

app.Run();

// Expose the entry point for WebApplicationFactory in integration tests.
public partial class Program { }
