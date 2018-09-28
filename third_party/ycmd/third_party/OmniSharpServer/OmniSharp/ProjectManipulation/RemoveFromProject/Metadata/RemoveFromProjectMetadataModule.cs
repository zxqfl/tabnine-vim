using Nancy.Metadata.Module;
using Nancy.Swagger;

namespace OmniSharp.ProjectManipulation.RemoveFromProject.Metadata
{
    public class RemoveFromProjectMetadataModule : MetadataModule<SwaggerRouteData>
    {
        public RemoveFromProjectMetadataModule()
        {
            Describe["RemoveFromProject"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/removefromproject")
                .BodyParam<RemoveFromProjectRequest>()
                .Response(200)
                .Model<bool>());
        }
    }
}
