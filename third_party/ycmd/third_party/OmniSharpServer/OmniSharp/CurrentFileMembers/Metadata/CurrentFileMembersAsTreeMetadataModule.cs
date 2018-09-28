using Nancy.Metadata.Module;
using Nancy.Swagger;

namespace OmniSharp.CurrentFileMembers.Metadata
{
    public class CurrentFileMembersAsTreeMetadataModule : MetadataModule<SwaggerRouteData>
    {
        public CurrentFileMembersAsTreeMetadataModule()
        {
            Describe["CurrentFileMembersAsTree"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/currentfilemembersastree")
                .BodyParam<CurrentFileMembersRequest>()
                .Response(200)
                .Model<CurrentFileMembersAsTreeResponse>());
        }
    }
}
