using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;
using OmniSharp.Common;
using OmniSharp.Solution;

namespace OmniSharp.FindSymbols
{
    public class FindSymbolsHandler
    {
        private readonly ISolution _solution;

        public FindSymbolsHandler(ISolution solution)
        {
            _solution = solution;
        }

        /// <summary>
        /// Find all symbols that only exist within the solution source tree
        /// </summary>
        public QuickFixResponse FindAllSymbols()
        {
            IEnumerable<IUnresolvedMember> types = 
                _solution.Projects.SelectMany(
                    project => project.ProjectContent.GetAllTypeDefinitions().SelectMany(t => t.Members));

            var quickfixes = types.Select(t => new QuickFix
                {
                    Text = t.Name + "\t(in " + t.DeclaringTypeDefinition.FullTypeName + ")",
                    FileName = t.UnresolvedFile.FileName,
                    Column = t.Region.BeginColumn,
                    Line = t.Region.BeginLine
                });

            return new QuickFixResponse(quickfixes);
        }
    }
}
