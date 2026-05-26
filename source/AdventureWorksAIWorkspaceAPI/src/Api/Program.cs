using AdventureWorksAIWorkspaceAPI.Api;
using AdventureWorksAIWorkspaceAPI.Application;
using AdventureWorksAIWorkspaceAPI.Infrastructure;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddApiServices();
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);

    Log.Information("Starting AdventureWorksAIWorkspace API");

    var app = builder.Build();

    app.UseApiServices();

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "AdventureWorksAIWorkspace API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
