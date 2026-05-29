using System.Text;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;
using AdventureWorksAIWorkspaceAPI.Application.WeatherForecasts;
using AdventureWorksAIWorkspaceAPI.Infrastructure.AdventureWorks;
using AdventureWorksAIWorkspaceAPI.Infrastructure.Database;
using AdventureWorksAIWorkspaceAPI.Infrastructure.Identity;
using AdventureWorksAIWorkspaceAPI.Infrastructure.OpenAi;
using AdventureWorksAIWorkspaceAPI.Infrastructure.Services;
using AdventureWorksAIWorkspaceAPI.Infrastructure.WeatherForecasts;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AdventureWorksAIWorkspaceAPI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        Guard.Against.Null(services);
        Guard.Against.Null(configuration);

        services.AddDatabaseServices(configuration);
        services.AddIdentityServices(configuration);
        services.AddAdventureWorksServices(configuration);
        services.AddOpenAiServices(configuration);
        services.AddSingleton<IWeatherForecastProvider, SampleWeatherForecastProvider>();
        services.AddSingleton<ISqlSafetyValidator, SqlSafetyValidator>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }

    private static IServiceCollection AddOpenAiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OpenAiOptions>(configuration.GetSection(OpenAiOptions.SectionName));

        services.AddHttpClient<IAiChatClient, OpenAiChatClient>((sp, httpClient) =>
        {
            OpenAiOptions options = sp.GetRequiredService<IOptions<OpenAiOptions>>().Value;
            httpClient.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        services.AddScoped<IAiSqlGenerator, AiSqlGenerator>();

        return services;
    }

    private static IServiceCollection AddAdventureWorksServices(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("AdventureWorksDatabase");
        Guard.Against.NullOrWhiteSpace(connectionString);

        services.Configure<AdventureWorksQueryOptions>(configuration.GetSection(AdventureWorksQueryOptions.SectionName));

        services.AddScoped<IAdventureWorksQueryExecutor>(sp =>
        {
            AdventureWorksQueryOptions options = sp.GetRequiredService<IOptions<AdventureWorksQueryOptions>>().Value;
            return new DapperAdventureWorksQueryExecutor(connectionString, options);
        });

        return services;
    }

    private static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("AdventureWorksAIWorkspaceDatabase");
        Guard.Against.NullOrWhiteSpace(connectionString);

        services.AddDbContext<AppDbContext>((sp, opt) =>
        {
            opt.UseSqlServer(connectionString);
        });

        return services;
    }

    private static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<InitialAdminOptions>(configuration.GetSection(InitialAdminOptions.SectionName));
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<AppDbContextInitializer>();

        JwtOptions jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section '{JwtOptions.SectionName}'");

        Guard.Against.NullOrWhiteSpace(jwtOptions.Issuer, $"{JwtOptions.SectionName}:{nameof(JwtOptions.Issuer)}");
        Guard.Against.NullOrWhiteSpace(jwtOptions.Audience, $"{JwtOptions.SectionName}:{nameof(JwtOptions.Audience)}");
        Guard.Against.NullOrWhiteSpace(jwtOptions.SigningKey, $"{JwtOptions.SectionName}:{nameof(JwtOptions.SigningKey)}");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization();

        return services;
    }
}
