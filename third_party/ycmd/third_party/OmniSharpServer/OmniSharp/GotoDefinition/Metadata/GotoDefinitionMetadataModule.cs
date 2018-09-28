using Nancy.Metadata.Module;
using Nancy.Swagger;

namespace OmniSharp.GotoDefinition.Metadata
{
    public class GotoDefinitionMetadataModule : MetadataModule<SwaggerRouteData>
    {
        public GotoDefinitionMetadataModule()
        {
            Describe["GoToDefinition"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/gotodefinition")
                .Summary("Retrieve the location of the definition of the symbol at the specified location")
                .BodyParam<GotoDefinitionRequest>(null, true)
                .Response(200)
                .Model<GotoDefinitionResponse>());
        }
    }
}
