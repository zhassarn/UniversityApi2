using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace UniversityApi.Swagger;

/// <summary>Әр API нұсқасы үшін жеке OpenAPI құжаты (v1, v2).</summary>
public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => _provider = provider;

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = "University Course Management API",
                Version = description.ApiVersion.ToString(),
                Description = description.ApiVersion.MajorVersion == 1
                    ? "v1: базалық контракт (курс: teacherId + teacherName)."
                    : "v2: кеңейтілген контракт (курс: оқытушы объектісі + жазылған студенттер саны)."
            });
        }
    }
}
