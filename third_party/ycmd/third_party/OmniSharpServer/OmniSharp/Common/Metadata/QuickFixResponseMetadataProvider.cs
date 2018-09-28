using Nancy.Swagger;
using Nancy.Swagger.Services;

namespace OmniSharp.Common.Metadata
{
    public class QuickFixResponseMetadataProvider : ISwaggerModelDataProvider
    {
        public SwaggerModelData GetModelData()
        {
            return SwaggerModelData.ForType<QuickFixResponse>(builder => builder
                .Property(p => p.QuickFixes).Description("A list of quick fixes"));
        }
    }
}
