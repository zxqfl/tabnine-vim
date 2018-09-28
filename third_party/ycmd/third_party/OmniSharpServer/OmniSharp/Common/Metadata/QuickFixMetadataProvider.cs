using Nancy.Swagger;
using Nancy.Swagger.Services;

namespace OmniSharp.Common.Metadata
{
    public class QuickFixMetadataProvider : ISwaggerModelDataProvider
    {
        public SwaggerModelData GetModelData()
        {
            return SwaggerModelData.ForType<QuickFix>(builder =>
            {
                builder.Property(p => p.LogLevel).Enum("Warning", "Error");
                builder.Property(p => p.EndLine).Default(-1);
                builder.Property(p => p.EndColumn).Default(-1);
            });
        }
    }
}
