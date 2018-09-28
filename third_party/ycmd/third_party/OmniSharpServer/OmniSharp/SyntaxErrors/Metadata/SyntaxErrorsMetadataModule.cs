using Nancy.Metadata.Module;
using Nancy.Swagger;
using OmniSharp.Common;

namespace OmniSharp.SyntaxErrors.Metadata
{
    public class SyntaxErrorsMetadataModule : MetadataModule<SwaggerRouteData>
    {
        public SyntaxErrorsMetadataModule()
        {
            Describe["SyntaxErrors"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/syntaxerrors")
                .BodyParam<Request>()
                .Response(200)
                .Model<SyntaxErrorsResponse>());
        }
    }
}
