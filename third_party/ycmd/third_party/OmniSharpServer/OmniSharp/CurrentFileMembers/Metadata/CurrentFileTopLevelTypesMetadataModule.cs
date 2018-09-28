using System.Collections.Generic;
using Nancy.Metadata.Module;
using Nancy.Swagger;
using OmniSharp.Common;

namespace OmniSharp.CurrentFileMembers.Metadata
{
    public class CurrentFileTopLevelTypesMetadataModule : MetadataModule<SwaggerRouteData>
    {
        public CurrentFileTopLevelTypesMetadataModule()
        {
            Describe["CurrentFileTopLevelTypes"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/currentfiletopleveltypes")
                .BodyParam<CurrentFileMembersRequest>()
                .Response(200)
                .Model<IEnumerable<QuickFix>>());
        }
    }
}
