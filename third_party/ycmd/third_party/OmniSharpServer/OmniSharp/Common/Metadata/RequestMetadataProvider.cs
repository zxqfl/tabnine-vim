using Nancy.Swagger;
using Nancy.Swagger.Services;

namespace OmniSharp.Common.Metadata
{
    public class RequestMetadataProvider : ISwaggerModelDataProvider
    {
        public SwaggerModelData GetModelData()
        {
            return SwaggerModelData.ForType<Request>(builder =>
            {
                builder.Property(p => p.Buffer).Required(true);
				builder.Property(p => p.FileName).Required(true);
				builder.Property(p => p.Line).Required(true);
				builder.Property(p => p.Column).Required(true);
            });
        }
    }
}
