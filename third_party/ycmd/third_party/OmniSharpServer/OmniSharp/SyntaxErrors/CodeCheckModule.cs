using Nancy;
using Nancy.ModelBinding;
using OmniSharp.Common;
using Request = OmniSharp.Common.Request;

namespace OmniSharp.SyntaxErrors
{
    public class CodeCheckModule : NancyModule
    {
        public CodeCheckModule(CodeCheckHandler handler)
        {
            Post["CodeCheck", "/codecheck"] = x =>
                {
                    var req = this.Bind<Request>();
                    var res = handler.CodeCheck(req);
                    return Response.AsJson(new QuickFixResponse(res));
                };
        }
    }
}