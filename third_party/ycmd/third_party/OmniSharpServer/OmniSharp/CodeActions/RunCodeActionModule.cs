using Nancy;
using Nancy.ModelBinding;

namespace OmniSharp.CodeActions
{
    public class RunCodeActionModule : NancyModule
    {
        public RunCodeActionModule(GetCodeActionsHandler codeActionsHandler)
        {
            Post["RunCodeAction", "/runcodeaction"] = x =>
                {
                    var req = this.Bind<CodeActionRequest>();
                    var res = codeActionsHandler.RunCodeAction(req);
                    return Response.AsJson(res);
                };
        }
    }
}
