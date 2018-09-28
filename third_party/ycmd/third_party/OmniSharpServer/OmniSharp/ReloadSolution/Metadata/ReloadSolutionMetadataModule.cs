using Nancy.Metadata.Module;
using Nancy.Swagger;

namespace OmniSharp.ReloadSolution.Metadata
{
    public class ReloadSolutionMetadataModule : MetadataModule<SwaggerRouteData>
    {
        public ReloadSolutionMetadataModule()
        {
            Describe["ReloadSolution"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/reloadsolution")
                .Response(200)
                .Model<bool>());
        }
    }
}
