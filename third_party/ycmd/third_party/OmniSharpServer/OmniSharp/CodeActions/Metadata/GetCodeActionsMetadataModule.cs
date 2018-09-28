using Nancy.Metadata.Module;
using Nancy.Swagger;

namespace OmniSharp.CodeActions.Metadata
{
    public class GetCodeActionsMetadataModule : MetadataModule<SwaggerRouteData>
    {

        public GetCodeActionsMetadataModule()
        {
            Describe["GetCodeActions"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/getcodeactions")
                .BodyParam<CodeActionRequest>()
                .Response(200)
                .Model<GetCodeActionsResponse>());
        }

    }
}
