using Nancy.Metadata.Module;
using Nancy.Swagger;
using OmniSharp.Common;

namespace OmniSharp.SyntaxErrors.Metadata
{
    public class CodeCheckMetadataModule : MetadataModule<SwaggerRouteData>
    {
        public CodeCheckMetadataModule()
        {
            Describe["CodeCheck"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/codecheck")
                .BodyParam<Request>()
                .Response(200)
                .Model<QuickFixResponse>());
        }
    }
}
