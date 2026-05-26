using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AdventureWorksAIWorkspaceAPI.Api.OpenApi;

internal sealed class RequireNonNullableSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema concrete || concrete.Properties is null)
        {
            return;
        }

        concrete.Required ??= new HashSet<string>();

        foreach (var property in concrete.Properties)
        {
            var isNullable = (property.Value.Type & JsonSchemaType.Null) == JsonSchemaType.Null;
            if (!isNullable && !concrete.Required.Contains(property.Key))
            {
                concrete.Required.Add(property.Key);
            }
        }
    }
}
