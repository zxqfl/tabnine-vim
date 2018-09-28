using Nancy.Metadata.Module;
using Nancy.Swagger;

namespace OmniSharp.Overrides.Metadata
{
    public class RunOverrideTargetMetadataModule : MetadataModule<SwaggerRouteData>
    {
        public RunOverrideTargetMetadataModule()
        {
            Describe["RunOverrideTarget"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/runoverridetarget")
                .BodyParam<RunOverrideTargetRequest>()
                .Response(200)
                .Model<RunOverrideTargetResponse>());
        }
    }
}
