using Nancy.Metadata.Module;
using Nancy.Swagger;

namespace OmniSharp.AutoComplete.Metadata
{
    public class AutoCompleteMetadataModule : MetadataModule<SwaggerRouteData>
    {

        public AutoCompleteMetadataModule()
        {
            Describe["AutoComplete"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/autocomplete")
                .Summary("Get completions at the text location")
                .BodyParam<AutoCompleteRequest>(null, true)
                .Response(200)
                .Model<AutoCompleteResponse>());
        }
    }
}
