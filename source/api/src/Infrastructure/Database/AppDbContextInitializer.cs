using AdventureWorksAIWorkspace.Domain.Reports;
using AdventureWorksAIWorkspace.Infrastructure.Database.Seeding;
using AdventureWorksAIWorkspace.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AdventureWorksAIWorkspace.Infrastructure.Database;

public static class InitializerExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initializer = scope.ServiceProvider.GetRequiredService<AppDbContextInitializer>();

        await initializer.InitialiseAsync();
        await initializer.SeedAsync();
    }
}

public class AppDbContextInitializer(
    ILogger<AppDbContextInitializer> logger,
    AppDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IOptions<InitialAdminOptions> initialAdminOptions,
    IWebHostEnvironment environment)
{
    private const string SampleAdminUserName = "Admin";

    // On a fresh dev machine the SQL Server (e.g. a Docker container) is often still starting up
    // when the API boots, so the first connection attempts fail. Wait and retry instead of crashing.
    private const int DevelopmentMigrationAttempts = 12;
    private static readonly TimeSpan DevelopmentMigrationRetryDelay = TimeSpan.FromSeconds(3);

    private readonly InitialAdminOptions _initialAdminOptions = initialAdminOptions.Value;

    public async Task InitialiseAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (environment.IsDevelopment())
            {
                await MigrateWithRetryAsync(cancellationToken);
            }
            else
            {
                await context.Database.MigrateAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initialising the database");
            throw;
        }
    }

    /// <summary>
    /// Applies all pending migrations on a development machine, retrying while the database is not
    /// yet reachable (for example a SQL Server container that is still starting up).
    /// </summary>
    private async Task MigrateWithRetryAsync(CancellationToken cancellationToken)
    {
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                await context.Database.MigrateAsync(cancellationToken);

                IEnumerable<string> applied = await context.Database.GetAppliedMigrationsAsync(cancellationToken);
                logger.LogInformation(
                    "Database migrated successfully; {Count} migration(s) applied", applied.Count());
                return;
            }
            catch (Exception ex) when (ex is not OperationCanceledException && attempt < DevelopmentMigrationAttempts)
            {
                logger.LogWarning(
                    ex,
                    "Database not ready while applying migrations (attempt {Attempt}/{MaxAttempts}); retrying in {Delay}s",
                    attempt,
                    DevelopmentMigrationAttempts,
                    DevelopmentMigrationRetryDelay.TotalSeconds);

                await Task.Delay(DevelopmentMigrationRetryDelay, cancellationToken);
            }
        }
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SeedRolesAsync();
            await SeedInitialAdminAsync();
            await SeedSampleReportForAdminAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private async Task SeedRolesAsync()
    {
        string[] roles = [RoleNames.Admin, RoleNames.User];

        foreach (string roleName in roles)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            IdentityResult result = await roleManager.CreateAsync(new ApplicationRole { Name = roleName });

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create role '{roleName}': {FormatErrors(result.Errors)}");
            }

            logger.LogInformation("Created role {RoleName}", roleName);
        }
    }

    private async Task SeedInitialAdminAsync()
    {
        if (!_initialAdminOptions.Enabled)
        {
            logger.LogInformation("Initial admin seeding is disabled");
            return;
        }

        if (string.IsNullOrWhiteSpace(_initialAdminOptions.Email))
        {
            throw new InvalidOperationException(
                $"{InitialAdminOptions.SectionName}:Email must be configured when initial admin seeding is enabled");
        }

        if (string.IsNullOrWhiteSpace(_initialAdminOptions.TemplatePassword))
        {
            throw new InvalidOperationException(
                $"{InitialAdminOptions.SectionName}:TemplatePassword must be configured (use User Secrets or environment variables) when initial admin seeding is enabled");
        }

        if (await userManager.FindByEmailAsync(_initialAdminOptions.Email) is not null)
        {
            logger.LogInformation("Initial admin {Email} already exists, skipping bootstrap", _initialAdminOptions.Email);
            return;
        }

        ApplicationUser admin = new()
        {
            UserName = string.IsNullOrWhiteSpace(_initialAdminOptions.UserName)
                ? _initialAdminOptions.Email
                : _initialAdminOptions.UserName,
            Email = _initialAdminOptions.Email,
            EmailConfirmed = true,
            IsFirstLogin = true
        };

        IdentityResult createResult = await userManager.CreateAsync(admin, _initialAdminOptions.TemplatePassword);

        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to create initial admin: {FormatErrors(createResult.Errors)}");
        }

        IdentityResult roleResult = await userManager.AddToRoleAsync(admin, RoleNames.Admin);

        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to assign {RoleNames.Admin} role to initial admin: {FormatErrors(roleResult.Errors)}");
        }

        logger.LogInformation("Initial admin {Email} created and assigned to {Role} role", admin.Email, RoleNames.Admin);
    }

    private async Task SeedSampleReportForAdminAsync(CancellationToken cancellationToken)
    {
        ApplicationUser? admin = await userManager.FindByNameAsync(SampleAdminUserName);
        if (admin is null)
        {
            return;
        }

        bool adminHasReport = await context.Reports
            .AnyAsync(report => report.UserId == admin.Id, cancellationToken);
        if (adminHasReport)
        {
            return;
        }

        Report report = SampleReportFactory.Create(admin.Id);

        context.Reports.Add(report);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded sample report for admin user {UserName}", admin.UserName);
    }

    private static string FormatErrors(IEnumerable<IdentityError> errors) =>
        string.Join("; ", errors.Select(e => $"{e.Code}: {e.Description}"));
}
