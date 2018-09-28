using Nancy;
using Nancy.ModelBinding;

namespace OmniSharp.TestContextInformation
{
    public class GetTestContextModule : NancyModule
    {
        public GetTestContextModule(GetTestContextHandler handler)
        {
            Post["/gettestcontext"] = x =>
                {
                    var request = this.Bind<TestCommandRequest>();
                    return Response.AsJson(handler.GetTestContextResponse(request));
                };
        }
    }
}