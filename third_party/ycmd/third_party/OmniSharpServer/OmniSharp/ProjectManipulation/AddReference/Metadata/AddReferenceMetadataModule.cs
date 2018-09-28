using Nancy.Metadata.Module;
using Nancy.Swagger;

namespace OmniSharp.ProjectManipulation.AddReference.Metadata
{
    public class AddReferenceMetadataModule : MetadataModule<SwaggerRouteData>
    {
        public AddReferenceMetadataModule()
        {
            Describe["AddReference"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/addreference")
                .BodyParam<AddReferenceRequest>()
                .Response(200)
                .Model<AddReferenceResponse>());
        }
    }
}
