using Nancy.Metadata.Module;
using Nancy.Swagger;
using OmniSharp.Common;

namespace OmniSharp.UpdateBuffer.Metadata
{
    public class UpdateBufferMetadataModule : MetadataModule<SwaggerRouteData>
    {
        public UpdateBufferMetadataModule()
        {
            Describe["UpdateBuffer"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/updatebuffer")
                .BodyParam<Request>()
                .Response(200)
                .Model<bool>());
        }
    }
}
