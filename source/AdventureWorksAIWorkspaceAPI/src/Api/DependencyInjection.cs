using AdventureWorksAIWorkspaceAPI.Api.ExceptionHandling;
using AdventureWorksAIWorkspaceAPI.Api.OpenApi;
using AdventureWorksAIWorkspaceAPI.Application;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;
using Ardalis.GuardClauses;
using Serilog;
using Serilog.Events;
using Wolverine;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;

namespace AdventureWorksAIWorkspaceAPI.Api;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddApiServices(this WebApplicationBuilder builder)
    {
        Guard.Against.Null(builder);

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        builder.Host.UseSerilog((context, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext();
        });

        builder.Host.UseWolverine(options =>
        {
            options.CodeGeneration.AlwaysUseServiceLocationFor<IAdventureWorksQueryExecutor>();
            options.CodeGeneration.AlwaysUseServiceLocationFor<IAiSqlGenerator>();
            options.CodeGeneration.AlwaysUseServiceLocationFor<IReportRepository>();
            options.CodeGeneration.AlwaysUseServiceLocationFor<ISqlSafetyValidator>();
            options.CodeGeneration.AlwaysUseServiceLocationFor<IUserService>();
            options.AddApplicationServices();
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddExceptionHandler<ApiExceptionHandler>();
        builder.Services.AddHealthChecks();
        builder.Services.AddProblemDetails();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SupportNonNullableReferenceTypes();
            options.UseAllOfToExtendReferenceSchemas();
            options.SchemaFilter<RequireNonNullableSchemaFilter>();
        });
        builder.Services.AddWolverineHttp();

        return builder;
    }

    public static WebApplication UseApiServices(this WebApplication app)
    {
        Guard.Against.Null(app);

        app.UseExceptionHandler();

        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "Handled {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.GetLevel = (httpContext, _, exception) =>
                exception is not null || httpContext.Response.StatusCode >= StatusCodes.Status500InternalServerError
                    ? LogEventLevel.Error
                    : LogEventLevel.Information;
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            };
        });

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.DocumentTitle = "AdventureWorksAIWorkspace API";
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "AdventureWorksAIWorkspace API v1");
            });
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapWolverineEndpoints(options =>
        {
            options.UseFluentValidationProblemDetailMiddleware();
        });

        return app;
    }
}
