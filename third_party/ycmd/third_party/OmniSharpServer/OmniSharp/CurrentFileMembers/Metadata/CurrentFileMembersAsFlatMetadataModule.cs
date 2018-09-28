using System.Collections.Generic;
using Nancy.Metadata.Module;
using Nancy.Swagger;
using OmniSharp.Common;

namespace OmniSharp.CurrentFileMembers.Metadata
{
    public class CurrentFileMembersAsFlatMetadataModule : MetadataModule<SwaggerRouteData>
    {
        public CurrentFileMembersAsFlatMetadataModule()
        {
            Describe["CurrentFileMembersAsFlat"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/currentfilemembersasflat")
                .BodyParam<CurrentFileMembersRequest>()
                .Response(200)
                .Model<IEnumerable<QuickFix>>());
        }
    }
}
