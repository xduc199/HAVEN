using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Haven.Infrastructure.Identity;

public static class IdentityBootstrapper
{
    private const string UserRole = "User";
    private const string AdminRole = "Admin";

    public static async Task ProvisionConfiguredAdminAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        var email = configuration["Haven:AdminBootstrap:Email"];
        var password = configuration["Haven:AdminBootstrap:Password"];

        if (string.IsNullOrWhiteSpace(email) &&
            string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Both Haven:AdminBootstrap:Email and Haven:AdminBootstrap:Password must be configured together.");
        }

        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<HavenUser>>();

        await EnsureRoleAsync(roleManager, UserRole);
        await EnsureRoleAsync(roleManager, AdminRole);

        var normalizedEmail = email.Trim();
        var user = await userManager.FindByEmailAsync(normalizedEmail);
        if (user is null)
        {
            user = new HavenUser
            {
                UserName = normalizedEmail,
                Email = normalizedEmail,
                DisplayName = "Admin"
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not provision the configured administrator. Identity error codes: {string.Join(", ", createResult.Errors.Select(error => error.Code))}");
            }
        }

        foreach (var role in new[] { UserRole, AdminRole })
        {
            if (await userManager.IsInRoleAsync(user, role))
            {
                continue;
            }

            var roleResult = await userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not assign role '{role}' to the configured administrator. Identity error codes: {string.Join(", ", roleResult.Errors.Select(error => error.Code))}");
            }
        }
    }

    private static async Task EnsureRoleAsync(
        RoleManager<IdentityRole> roleManager,
        string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        var result = await roleManager.CreateAsync(new IdentityRole(roleName));
        if (!result.Succeeded && !await roleManager.RoleExistsAsync(roleName))
        {
            throw new InvalidOperationException(
                $"Could not create the required role '{roleName}'. Identity error codes: {string.Join(", ", result.Errors.Select(error => error.Code))}");
        }
    }
}
