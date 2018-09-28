using Nancy.Metadata.Module;
using Nancy.Swagger;
using OmniSharp.Common;

namespace OmniSharp.FindTypes.Metadata
{
    public class FindTypesMetadataModule : MetadataModule<SwaggerRouteData>
    {
        public FindTypesMetadataModule()
        {
            Describe["FindTypes"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/findtypes")
                .Response(200)
                .Model<QuickFixResponse>());
        }
    }
}
