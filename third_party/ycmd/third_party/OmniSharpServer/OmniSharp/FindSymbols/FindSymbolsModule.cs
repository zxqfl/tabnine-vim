using Nancy;

namespace OmniSharp.FindSymbols
{
    public class FindSymbolsModule : NancyModule
    {
        public FindSymbolsModule(FindSymbolsHandler handler)
        {
            Post["FindSymbols", "/findsymbols"] = x =>
                {
                    var res = handler.FindAllSymbols();
                    return Response.AsJson(res);
                };
        }
    }
}