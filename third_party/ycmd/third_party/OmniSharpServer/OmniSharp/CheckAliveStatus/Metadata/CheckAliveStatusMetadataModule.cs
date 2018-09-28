using Nancy.Metadata.Module;
using Nancy.Swagger;

namespace OmniSharp.CheckAliveStatus.Metadata
{
    public class CheckAliveStatusMetadataModule : MetadataModule<SwaggerRouteData>
    {
        public CheckAliveStatusMetadataModule()
        {
            Describe["CheckAliveStatus"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/checkalivestatus")
                .Response(200)
                .Model<bool>());
        }
    }
}
