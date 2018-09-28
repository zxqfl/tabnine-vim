using Nancy.Metadata.Module;
using Nancy.Swagger;

namespace OmniSharp.StopServer.Metadata
{
    public class StopServerMetadataModule : MetadataModule<SwaggerRouteData>
    {
        public StopServerMetadataModule()
        {
            Describe["StopServer"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/stopserver")
                .Response(200)
                .Model<bool>());
        }
    }
}
