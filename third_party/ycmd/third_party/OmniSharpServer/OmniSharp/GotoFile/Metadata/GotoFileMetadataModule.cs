using Nancy.Metadata.Module;
using Nancy.Swagger;
using OmniSharp.Common;

namespace OmniSharp.GotoFile.Metadata
{
    public class GotoFileMetadataModule : MetadataModule<SwaggerRouteData>
    {
        public GotoFileMetadataModule()
        {
            Describe["GotoFile"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/gotofile")
                .Response(200)
                .Model<QuickFixResponse>());
        }
    }
}
