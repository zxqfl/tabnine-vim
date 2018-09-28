using Nancy.Swagger;
using Nancy.Swagger.Services;

namespace OmniSharp.AutoComplete.Metadata
{
    public class AutoCompleteResponseMetadataProvider : ISwaggerModelDataProvider
    {
        public SwaggerModelData GetModelData()
        {
            return SwaggerModelData.ForType<AutoCompleteResponse>(builder =>
            {
                builder.Description("Response");
                builder.Property(r => r.MethodHeader).Default(null); //?
            });
        }
    }
}
