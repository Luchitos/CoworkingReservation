using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text.Json.Serialization;

namespace CoworkingReservation.API.Filters
{
    public class SwaggerIgnoreSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema == null || context.Type == null)
                return;

            var excludedProperties = context.Type.GetProperties()
                .Where(t => t.GetCustomAttribute<JsonIgnoreAttribute>() != null);

            foreach (var excludedProperty in excludedProperties)
            {
                var propertyName = excludedProperty.Name;
                // Convert first letter to lowercase for camelCase
                var camelCasePropertyName = char.ToLowerInvariant(propertyName[0]) + propertyName.Substring(1);
                
                // Remove properties from schema that have JsonIgnore
                if (schema.Properties.ContainsKey(propertyName))
                {
                    schema.Properties.Remove(propertyName);
                }
                if (schema.Properties.ContainsKey(camelCasePropertyName))
                {
                    schema.Properties.Remove(camelCasePropertyName);
                }
            }
        }
    }
} 