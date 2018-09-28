using Nancy.Metadata.Module;
using Nancy.Swagger;

namespace OmniSharp.CodeActions.Metadata
{
    public class RunCodeActionMetadataModule : MetadataModule<SwaggerRouteData>
    {
        public RunCodeActionMetadataModule()
        {
            Describe["RunCodeAction"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/runcodeaction")
                .BodyParam<CodeActionRequest>()
                .Response(200)
                .Model<RunCodeActionsResponse>());
        }
    }
}
