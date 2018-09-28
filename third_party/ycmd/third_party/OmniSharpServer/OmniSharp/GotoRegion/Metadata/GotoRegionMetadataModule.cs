using Nancy.Metadata.Module;
using Nancy.Swagger;
using OmniSharp.Common;

namespace OmniSharp.GotoRegion.Metadata
{
    public class GotoRegionMetadataModule : MetadataModule<SwaggerRouteData>
    {
        public GotoRegionMetadataModule()
        {
            Describe["GotoRegion"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/gotoregion")
                .BodyParam<Request>()
                .Response(200)
                .Model<QuickFixResponse>());
        }
    }
}
