using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AdventureWorksAIWorkspaceAPI.Api.OpenApi;

/// <summary>
/// Renders enum schemas as strings (the enum member names) so the OpenAPI document matches the
/// runtime <see cref="System.Text.Json.Serialization.JsonStringEnumConverter"/> serialization.
/// Without this, Swashbuckle can describe enums as integers, which leaks into generated clients.
/// </summary>
internal sealed class EnumAsStringSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum || schema is not OpenApiSchema concrete)
        {
            return;
        }

        concrete.Type = JsonSchemaType.String;
        concrete.Format = null;
        concrete.Properties?.Clear();

        concrete.Enum = Enum.GetNames(context.Type)
            .Select(name => (JsonNode)JsonValue.Create(name))
            .ToList();
    }
}
