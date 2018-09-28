using Nancy;
using Nancy.ModelBinding;
using OmniSharp.CodeIssues;

public class FixUsingsModule : NancyModule
{
    public FixUsingsModule(FixUsingsHandler fixUsingsHandler)
    {
        Post["FixUsings", "/fixusings"] = x =>
        {
            var req = this.Bind<OmniSharp.Common.Request>();
            var res = fixUsingsHandler.FixUsings(req);
            return Response.AsJson(res);
        };
    }
}
