using Nancy.Metadata.Module;
using Nancy.Swagger;
using OmniSharp.Common;

namespace OmniSharp.FindSymbols.Metadata
{
    public class FindSymbolsMetadataModule : MetadataModule<SwaggerRouteData>
    {
        public FindSymbolsMetadataModule()
        {
            Describe["FindSymbols"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/findsymbols")
                .Response(200)
                .Model<QuickFixResponse>());
        }
    }
}
