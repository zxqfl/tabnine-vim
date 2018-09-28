using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;

namespace OmniSharp.Solution
{
    public class ProjectFinder
    {
        readonly ISolution _solution;

        public ProjectFinder(ISolution solution)
        {
            _solution = solution;
        }

        public IEnumerable<IProject> FindProjectsReferencing(ITypeResolveContext context)
        {
            var contextAssemblyName = context.Compilation.MainAssembly.FullAssemblyName;
            System.Console.WriteLine(contextAssemblyName);

            IProject sourceProject = _solution.Projects.FirstOrDefault(p => p.ProjectContent.FullAssemblyName == contextAssemblyName);
            var projectsThatReferenceUsage = from p in _solution.Projects
            where p.References.Any(r => r.Resolve(context).FullAssemblyName == contextAssemblyName) || p == sourceProject
            select p;
            return projectsThatReferenceUsage;
        }
    }
}
