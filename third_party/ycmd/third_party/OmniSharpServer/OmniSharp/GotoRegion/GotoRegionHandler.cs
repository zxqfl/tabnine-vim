using System;
using System.Linq;
using System.Collections.Generic;
using ICSharpCode.NRefactory.CSharp;
using OmniSharp.AutoComplete;
using OmniSharp.Common;
using OmniSharp.Parser;

namespace OmniSharp.GotoRegion {

    public class GotoRegionHandler {
        private readonly BufferParser _parser;

        public GotoRegionHandler(BufferParser parser) {
            _parser = parser;
        }

        /// <summary>
        ///   Returns a representation of the current buffer's members
        ///   and their locations. The caller may build a UI that lets
        ///   the user navigate to them quickly.
        /// </summary>
        public QuickFixResponse
            GetFileRegions(Request request) {
            var context = new BufferContext(request, this._parser);

            var declarationCollector = new GetDirectivesAstVisitor();
            context.ParsedContent.SyntaxTree
                .AcceptVisitor(declarationCollector);

            var regions = declarationCollector.Directives
                .Where(d => d.Type == PreProcessorDirectiveType.Region
                       || d.Type == PreProcessorDirectiveType.Endregion)
                .Select(d => QuickFix.ForFirstLineInRegion
                        (d.GetRegion(), context.Document));

            return new QuickFixResponse(regions);
        }
    }
}
