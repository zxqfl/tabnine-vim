using Nancy.Metadata.Module;
using Nancy.Swagger;

namespace OmniSharp.Build.Metadata
{
    public class BuildCommandMetadataModule : MetadataModule<SwaggerRouteData>
    {
        public BuildCommandMetadataModule()
        {
            Describe["BuildCommand"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/buildcommand")
                .Response(200)
                .Model<string>());

            Describe["BuildTarget"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/buildtarget")
                .BodyParam<BuildTargetRequest>()
                .Response(200)
                .Model<BuildTargetResponse>());
        }
    }
}
