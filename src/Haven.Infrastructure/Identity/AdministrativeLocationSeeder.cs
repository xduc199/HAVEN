using System.Text.Json;
using Haven.Domain.Locations;
using Haven.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Haven.Infrastructure.Identity;

public static class AdministrativeLocationSeeder
{
    private const string DataFile = "vietnam-administrative-units-2025-07-01.json";

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var file = Path.Combine(AppContext.BaseDirectory, "Data", "Locations", DataFile);
        await using var stream = File.OpenRead(file);
        var dataset = await JsonSerializer.DeserializeAsync<LocationDataset>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellationToken)
            ?? throw new InvalidDataException("The administrative location dataset is empty.");

        if (dataset.Provinces.Count != 34 || dataset.Wards.Count != 3321 ||
            dataset.Provinces.Select(x => x.Code).Distinct(StringComparer.Ordinal).Count() != 34 ||
            dataset.Wards.Select(x => x.Code).Distinct(StringComparer.Ordinal).Count() != 3321 ||
            dataset.Wards.Any(ward => dataset.Provinces.All(province => province.Code != ward.ProvinceCode)))
            throw new InvalidDataException("The administrative location dataset failed code and parent validation.");

        var existingProvinceCount = await db.AdministrativeProvinces.CountAsync(cancellationToken);
        var existingWardCount = await db.AdministrativeWards.CountAsync(cancellationToken);
        if (existingProvinceCount == 0 && existingWardCount == 0)
        {
            db.AdministrativeProvinces.AddRange(dataset.Provinces.Select(x => AdministrativeProvince.Create(x.Code, x.Name)));
            db.AdministrativeWards.AddRange(dataset.Wards.Select(x => AdministrativeWard.Create(x.Code, x.ProvinceCode, x.Name, x.Type)));
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        if (existingProvinceCount != 34 || existingWardCount != 3321)
            throw new InvalidDataException("Existing administrative locations do not match the bundled official dataset; no data was overwritten.");

        var existingProvinces = await db.AdministrativeProvinces.AsNoTracking().ToDictionaryAsync(x => x.Code, cancellationToken);
        var existingWards = await db.AdministrativeWards.AsNoTracking().ToDictionaryAsync(x => x.Code, cancellationToken);
        if (dataset.Provinces.Any(x => !existingProvinces.TryGetValue(x.Code, out var current) || current.Name != x.Name) ||
            dataset.Wards.Any(x => !existingWards.TryGetValue(x.Code, out var current) || current.ProvinceCode != x.ProvinceCode || current.Name != x.Name || current.Type != x.Type))
            throw new InvalidDataException("Existing administrative location codes or parent relationships differ from the bundled dataset; no data was overwritten.");
    }

    private sealed record LocationDataset(List<ProvinceData> Provinces, List<WardData> Wards);
    private sealed record ProvinceData(string Code, string Name);
    private sealed record WardData(string Code, string Name, string ProvinceCode, string Type);
}
