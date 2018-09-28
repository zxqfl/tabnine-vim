using Nancy;
using Nancy.ModelBinding;
using OmniSharp.CodeActions;
using Request = OmniSharp.Common.Request;

namespace OmniSharp.CodeIssues
{
    public class CodeIssuesModule : NancyModule
    {
        public CodeIssuesModule(CodeIssuesHandler codeIssuesHandler)
        {
            Post["GetCodeIssues", "/getcodeissues"] = x =>
                {
                    var req = this.Bind<Request>();
                    var res = codeIssuesHandler.GetCodeIssues(req);
                    return Response.AsJson(res);
                };

            Post["FixCodeIssue", "/fixcodeissue"] = x =>
            {
                var req = this.Bind<RunCodeActionRequest>();
                var res = codeIssuesHandler.FixCodeIssue(req);
                return Response.AsJson(res);
            };
        }
    }
}
