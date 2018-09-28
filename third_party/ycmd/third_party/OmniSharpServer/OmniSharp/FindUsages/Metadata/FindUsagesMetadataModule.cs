using Nancy.Metadata.Module;
using Nancy.Swagger;
using OmniSharp.Common;

namespace OmniSharp.FindUsages.Metadata
{
    public class FindUsagesMetadataModule : MetadataModule<SwaggerRouteData>
    {
        public FindUsagesMetadataModule()
        {
            Describe["FindUsages"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/findusages")
                .BodyParam<FindUsagesRequest>()
                .Response(200)
                .Model<QuickFixResponse>());
        }
    }
}
