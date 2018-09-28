using System.Collections.Generic;
using ICSharpCode.NRefactory.CSharp;

namespace OmniSharp.GotoRegion {

    public class GetDirectivesAstVisitor : DepthFirstAstVisitor {
        public IList<PreProcessorDirective> Directives =
            new List<PreProcessorDirective>();

        public override void VisitPreProcessorDirective
            (PreProcessorDirective preProcessorDirective) {
            Directives.Add(preProcessorDirective);
        }
    }
}
