using Nancy.Metadata.Module;
using Nancy.Swagger;

namespace OmniSharp.LookupAllTypes.Metadata
{
    public class LookupAllTypesMetadataModule : MetadataModule<SwaggerRouteData>
    {
        public LookupAllTypesMetadataModule()
        {
            Describe["LookupAllTypes"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/lookupalltypes")
                .Response(200)
                .Model<LookupAllTypesResponse>());
        }
    }
}
