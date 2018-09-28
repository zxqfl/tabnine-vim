using Nancy.Metadata.Module;
using Nancy.Swagger;
using OmniSharp.CodeActions;
using OmniSharp.Common;

namespace OmniSharp.CodeIssues.Metadata
{
    public class CodeIssuesMetadataModule : MetadataModule<SwaggerRouteData>
    {
        public CodeIssuesMetadataModule()
        {
            Describe["GetCodeIssues"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/getcodeissues")
                .BodyParam<Request>()
                .Response(200)
                .Model<QuickFixResponse>());

            Describe["FixCodeIssue"] = desc => desc.AsSwagger(builder => builder
                .ResourcePath("/fixcodeissue")
                .BodyParam<RunCodeActionRequest>()
                .Response(200)
                .Model<RunCodeIssuesResponse>());
        }
    }
}
