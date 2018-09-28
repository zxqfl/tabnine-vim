using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using OmniSharp.Configuration;
using OmniSharp.FindUsages;
using OmniSharp.Parser;
using OmniSharp.Refactoring;
using OmniSharp.Solution;

namespace OmniSharp.Rename
{
    public class RenameHandler
    {
        private readonly ISolution _solution;
        private readonly BufferParser _bufferParser;
        private readonly FindUsagesHandler _findUsagesHandler;
		private readonly OmniSharpConfiguration _config;

        public RenameHandler(ISolution solution, BufferParser bufferParser, OmniSharpConfiguration config, FindUsagesHandler findUsagesHandler)
        {
            _solution = solution;
            _bufferParser = bufferParser;
            _findUsagesHandler = findUsagesHandler;
            _config = config;
        }

        public RenameResponse Rename(RenameRequest req)
        {
            var project = _solution.ProjectContainingFile(req.FileName);
            var syntaxTree = project.CreateParser().Parse(req.Buffer, req.FileName);
            var sourceNode = syntaxTree.GetNodeAt(req.Line, req.Column);
            if(sourceNode == null)
                return new RenameResponse();

            IEnumerable<AstNode> nodes = _findUsagesHandler.FindUsageNodes(req).ToArray();

            var response = new RenameResponse();

            var modfiedFiles = new List<ModifiedFileResponse>();
            response.Changes = modfiedFiles;

            foreach (IGrouping<string, AstNode> groupedNodes in nodes.GroupBy(n => n.GetRegion().FileName))
            {
                string fileName = groupedNodes.Key;
                OmniSharpRefactoringContext context;
                if (groupedNodes.Key != req.FileName)
                {
                    var file = _solution.GetFile(fileName);
                    var bufferParser = new BufferParser(_solution);
                    var content = bufferParser.ParsedContent(file.Document.Text, file.FileName);
                    var resolver = new CSharpAstResolver(content.Compilation, content.SyntaxTree, content.UnresolvedFile);
                    context = new OmniSharpRefactoringContext(file.Document, resolver);
                }
                else
                {
                    context = OmniSharpRefactoringContext.GetContext(_bufferParser, req);
                }
                string modifiedBuffer = null;

                var lastToFirstNodes = groupedNodes.OrderByDescending(n => n.EndLocation.Line)
                                                   .ThenByDescending(n => n.EndLocation.Column);

                foreach (var node in lastToFirstNodes)
                {
                    using (var script = new OmniSharpScript(context, _config))
                    {
                        script.Rename(node, req.RenameTo);
                        modifiedBuffer = script.CurrentDocument.Text;
                    }
                }

                if (modifiedBuffer != null)
                {
                    var modifiedFile = new ModifiedFileResponse
                    {
                        FileName
                        = fileName,
                        Buffer = modifiedBuffer
                    };
                    modfiedFiles.Add(modifiedFile);
                    response.Changes = modfiedFiles;

                    _bufferParser.ParsedContent(modifiedBuffer, fileName);
                    _solution.ProjectContainingFile(fileName).UpdateFile (fileName, modifiedBuffer);
                }
            }

            return response;
        }
    }
}
