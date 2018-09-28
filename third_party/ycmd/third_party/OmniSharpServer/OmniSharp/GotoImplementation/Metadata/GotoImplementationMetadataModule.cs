using Nancy.Metadata.Module;
using Nancy.Swagger;
using OmniSharp.Common;

namespace OmniSharp.GotoImplementation.Metadata
{
    public class GotoImplementationMetadataModule : MetadataModule<SwaggerRouteData>
    {
        public GotoImplementationMetadataModule()
        {
            Describe["FindImplementations"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/findimplementations")
                .BodyParam<GotoImplementationRequest>()
                .Response(200)
                .Model<QuickFixResponse>());
        }
    }
}
