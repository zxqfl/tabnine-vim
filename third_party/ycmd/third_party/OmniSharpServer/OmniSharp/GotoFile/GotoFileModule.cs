using Nancy;

namespace OmniSharp.GotoFile
{
    public class GotoFileModule : NancyModule
    {
        public GotoFileModule(GotoFileHandler gotoFileHandler)
        {
            Post["GotoFile", "/gotofile"] = x =>
            {
                var res = gotoFileHandler.GetSolutionFiles();
                return Response.AsJson(res);
            };
        }
    }
}
