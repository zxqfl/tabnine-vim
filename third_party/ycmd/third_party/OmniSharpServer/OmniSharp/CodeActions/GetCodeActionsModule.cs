using Nancy;
using Nancy.ModelBinding;
using Request = OmniSharp.Common.Request;

namespace OmniSharp.CodeActions
{
    public class GetCodeActionsModule : NancyModule
    {
        public GetCodeActionsModule(GetCodeActionsHandler getCodeActionsHandler)
        {
            Post["GetCodeActions", "/getcodeactions"] = x =>
                {
                    var req = this.Bind<CodeActionRequest>();
                    var res = getCodeActionsHandler.GetCodeActions(req);
                    return Response.AsJson(res);
                };
        }
    }
}
