using Nancy;
using Nancy.ModelBinding;
using Request = OmniSharp.Common.Request;

namespace OmniSharp.SemanticErrors
{
    public class SemanticErrorsModule : NancyModule
    {
        public SemanticErrorsModule(SemanticErrorsHandler semanticErrorsHandler)
        {
            Post["SemanticErrors", "/semanticerrors"] = x =>
                {
                    var req = this.Bind<Request>();
                    var res = semanticErrorsHandler.FindSemanticErrors(req);
                    return Response.AsJson(res);
                };
        }
    }
}
