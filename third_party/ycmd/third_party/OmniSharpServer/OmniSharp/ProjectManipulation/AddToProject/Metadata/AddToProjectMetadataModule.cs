using Nancy.Metadata.Module;
using Nancy.Swagger;

namespace OmniSharp.ProjectManipulation.AddToProject.Metadata
{
    public class AddToProjectMetadataModule : MetadataModule<SwaggerRouteData>
    {
        public AddToProjectMetadataModule()
        {
            Describe["AddToProject"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/addtoproject")
                .BodyParam<AddToProjectRequest>()
                .Response(200)
                .Model<bool>());
        }
    }
}
