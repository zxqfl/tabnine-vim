using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Semantics;
using OmniSharp.AutoComplete;
using OmniSharp.Parser;
using OmniSharp.Solution;

namespace OmniSharp.GotoDefinition
{
    public class GotoDefinitionHandler
    {
        private readonly BufferParser _bufferParser;

        public GotoDefinitionHandler(BufferParser bufferParser)
        {
            _bufferParser = bufferParser;
        }

        public GotoDefinitionResponse GetGotoDefinitionResponse(GotoDefinitionRequest request)
        {
            var res = _bufferParser.ParsedContent(request.Buffer, request.FileName);
            
            var loc = new TextLocation(request.Line, request.Column);

            ResolveResult resolveResult = ResolveAtLocation.Resolve(res.Compilation, res.UnresolvedFile, res.SyntaxTree, loc);
            var response = new GotoDefinitionResponse();
            if (resolveResult != null)
            {
                var region = resolveResult.GetDefinitionRegion();
                response.FileName = region.FileName == null ? null : region.FileName.ApplyPathReplacementsForClient();
                response.Line = region.BeginLine;
                response.Column = region.BeginColumn;
            }

            return response;
        }

    }
}
