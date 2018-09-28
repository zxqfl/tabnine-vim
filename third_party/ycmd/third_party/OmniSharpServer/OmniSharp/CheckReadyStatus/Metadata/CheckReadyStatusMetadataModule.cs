using Nancy.Metadata.Module;
using Nancy.Swagger;

namespace OmniSharp.CheckReadyStatus.Metadata
{
    public class CheckReadyStatusMetadataModule : MetadataModule<SwaggerRouteData>
    {

        public CheckReadyStatusMetadataModule()
        {
            Describe["CheckReadyStatus"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/checkreadystatus")
                .Response(200)
                .Model<bool>());
        }
    }
}
