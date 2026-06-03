using System.Text.Json.Serialization;
using AdventureWorksAIWorkspace.Api.ExceptionHandling;
using AdventureWorksAIWorkspace.Api.OpenApi;
using AdventureWorksAIWorkspace.Application;
using AdventureWorksAIWorkspace.Application.Common.Services.AdventureWorks;
using AdventureWorksAIWorkspace.Application.Common.Services.Ai;
using AdventureWorksAIWorkspace.Application.Common.Services.Auth;
using AdventureWorksAIWorkspace.Application.Common.Services.Reports;
using AdventureWorksAIWorkspace.Application.Common.Services.Sql;
using AdventureWorksAIWorkspace.Application.Common.Services.User;
using Ardalis.GuardClauses;
using Serilog;
using Serilog.Events;
using Wolverine;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;

namespace AdventureWorksAIWorkspace.Api;

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
                .Enrich.FromLogContext()
                // Requests cancelled by the client (a cancelled fetch, navigation, or a React
                // StrictMode remount in development) surface as an OperationCanceledException
                // through EF Core and Wolverine. These are not server faults, so drop any log
                // event carrying one — this also silences Wolverine's own "Invocation ... failed!"
                // error for cancelled message handlers.
                .Filter.ByExcluding(logEvent => logEvent.Exception is OperationCanceledException);
        });

        builder.Host.UseWolverine(options =>
        {
            options.CodeGeneration.AlwaysUseServiceLocationFor<IAdventureWorksQueryExecutor>();
            options.CodeGeneration.AlwaysUseServiceLocationFor<IAiSqlGenerator>();
            options.CodeGeneration.AlwaysUseServiceLocationFor<IReportChatPipeline>();
            options.CodeGeneration.AlwaysUseServiceLocationFor<IReportIntentClassifier>();
            options.CodeGeneration.AlwaysUseServiceLocationFor<IReportRepository>();
            options.CodeGeneration.AlwaysUseServiceLocationFor<IReportVisualizer>();
            options.CodeGeneration.AlwaysUseServiceLocationFor<ISqlSafetyValidator>();
            options.CodeGeneration.AlwaysUseServiceLocationFor<IAuthenticationService>();
            options.CodeGeneration.AlwaysUseServiceLocationFor<IUserManagementService>();
            options.AddApplicationServices();
        });

        // Serialize enums as their string names so the API contract is stable and self-describing
        // (for example "Ready" instead of 3). Wolverine HTTP and minimal APIs both use these options.
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
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
            // Mirror the runtime string-enum serialization in the OpenAPI document so generated
            // clients see string enums instead of integers.
            options.SchemaFilter<EnumAsStringSchemaFilter>();
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
            {
                // A request the client aborted (cancelled fetch, navigation) is not a server
                // error; keep it out of the error logs.
                if (exception is OperationCanceledException || httpContext.RequestAborted.IsCancellationRequested)
                {
                    return LogEventLevel.Debug;
                }

                return exception is not null
                       || httpContext.Response.StatusCode >= StatusCodes.Status500InternalServerError
                    ? LogEventLevel.Error
                    : LogEventLevel.Information;
            };
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
