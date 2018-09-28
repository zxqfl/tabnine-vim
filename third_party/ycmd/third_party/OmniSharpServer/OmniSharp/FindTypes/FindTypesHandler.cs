using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;
using OmniSharp.Common;
using OmniSharp.Solution;

namespace OmniSharp.FindTypes
{
    public class FindTypesHandler
    {
        readonly ISolution _solution;

        public FindTypesHandler(ISolution solution)
        {
            _solution = solution;
        }

        /// <summary>
        /// Find all types that only exist within the solution source tree
        /// </summary>
        public QuickFixResponse FindAllTypes()
        {
            IEnumerable<IUnresolvedTypeDefinition> types = 
                _solution.Projects.SelectMany(
                    project => project.ProjectContent.GetAllTypeDefinitions());

            var quickfixes = types.Select(t => new QuickFix
                {
                    Text = t.Name + "\t(in " + t.Namespace + ")",
                    FileName = t.UnresolvedFile.FileName,
                    Column = t.Region.BeginColumn,
                    Line = t.Region.BeginLine
                });

            return new QuickFixResponse(quickfixes);
        }
    }
}
