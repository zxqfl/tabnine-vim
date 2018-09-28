using Nancy.Metadata.Module;
using Nancy.Swagger;

namespace OmniSharp.CodeFormat.Metadata
{
    public class CodeFormatMetadataModule : MetadataModule<SwaggerRouteData>
    {
        public CodeFormatMetadataModule()
        {
            Describe["CodeFormat"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/codeformat")
                .BodyParam<CodeFormatRequest>()
                .Response(200)
                .Model<CodeFormatResponse>());
        }
    }
}
