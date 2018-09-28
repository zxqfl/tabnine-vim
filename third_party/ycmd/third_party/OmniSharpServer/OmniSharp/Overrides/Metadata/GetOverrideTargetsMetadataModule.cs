using Nancy.Metadata.Module;
using Nancy.Swagger;
using OmniSharp.Common;

namespace OmniSharp.Overrides.Metadata
{
    public class GetOverrideTargetsMetadataModule : MetadataModule<SwaggerRouteData>
    {
        public GetOverrideTargetsMetadataModule()
        {
            Describe["GetOverrideTargets"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/getoverridetargets")
                .BodyParam<Request>()
                .Response(200)
                .Model<GetOverrideTargetsResponse>());
        }
    }
}
