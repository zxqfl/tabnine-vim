using Nancy.Metadata.Module;
using Nancy.Swagger;

namespace OmniSharp.TypeLookup.Metadata
{
    public class TypeLookupMetadataModule : MetadataModule<SwaggerRouteData>
    {
        public TypeLookupMetadataModule()
        {
            Describe["TypeLookup"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/typelookup")
                .BodyParam<TypeLookupRequest>()
                .Response(200)
                .Model<TypeLookupResponse>());
        }
    }
}
