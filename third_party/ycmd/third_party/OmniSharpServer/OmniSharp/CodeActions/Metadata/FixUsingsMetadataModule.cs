using Nancy.Metadata.Module;
using Nancy.Swagger;
using OmniSharp.Common;

namespace OmniSharp.CodeActions.Metadata
{
    public class FixUsingsMetadataModule : MetadataModule<SwaggerRouteData>
    {
        public FixUsingsMetadataModule()
        {
            Describe["FixUsings"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/fixusings")
                .BodyParam<Request>()
                .Response(200)
                .Model<FixUsingsResponse>());
        }
    }
}
