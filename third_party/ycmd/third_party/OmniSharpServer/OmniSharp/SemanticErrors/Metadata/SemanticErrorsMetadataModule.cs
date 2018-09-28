using Nancy.Metadata.Module;
using Nancy.Swagger;
using OmniSharp.Common;

namespace OmniSharp.SemanticErrors.Metadata
{
    public class SemanticErrorsMetadataModule : MetadataModule<SwaggerRouteData>
    {
        public SemanticErrorsMetadataModule()
        {
            Describe["SemanticErrors"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/semanticerrors")
                .BodyParam<Request>()
                .Response(200)
                .Model<SemanticErrorsResponse>());
        }
    }
}
