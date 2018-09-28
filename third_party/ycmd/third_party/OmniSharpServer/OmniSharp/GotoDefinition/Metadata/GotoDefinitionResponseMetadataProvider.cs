using Nancy.Swagger;
using Nancy.Swagger.Services;

namespace OmniSharp.GotoDefinition.Metadata
{
    public class GotoDefinitionResponseMetadataProvider : ISwaggerModelDataProvider
    {
        public SwaggerModelData GetModelData()
        {
            return SwaggerModelData.ForType<GotoDefinitionResponse>(builder =>
            {
                builder.Description("Response");
                builder.Property(p => p.Line).Minimum(1);
                builder.Property(p => p.Column).Minimum(1);
                builder.Property(p => p.FileName);
            });
        }
    }
}
