using Nancy.Metadata.Module;
using Nancy.Swagger;

namespace OmniSharp.Rename.Metadata
{
    public class RenameMetadataModule : MetadataModule<SwaggerRouteData>
    {
        public RenameMetadataModule()
        {
            Describe["Rename"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/rename")
                .BodyParam<RenameRequest>()
                .Response(200)
                .Model<RenameResponse>());
        }
    }
}
