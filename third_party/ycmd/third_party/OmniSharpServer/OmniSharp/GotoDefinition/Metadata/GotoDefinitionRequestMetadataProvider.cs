using Nancy.Swagger;
using Nancy.Swagger.Services;

namespace OmniSharp.GotoDefinition.Metadata
{
    public class GotoDefinitionRequestMetadataProvider : ISwaggerModelDataProvider
    {
        public SwaggerModelData GetModelData()
        {
            return SwaggerModelData.ForType<GotoDefinitionRequest>(builder =>
            {
                builder.Description("request");
                builder.Property(p => p.Buffer).Required(true);
                builder.Property(p => p.FileName).Required(true);
                builder.Property(p => p.Line).Minimum(1).Required(true);
                builder.Property(p => p.Column).Minimum(1).Required(true);
            });
        }
    }
}
