using System;
using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;

namespace OmniSharp.Solution
{
    public class ProjectReference : IAssemblyReference
    {
        readonly ISolution _solution;
        readonly string _projectTitle;
		readonly Guid _projectGuid;

        public ProjectReference(ISolution solution, string projectTitle, Guid projectGuid)
        {
            
            _solution = solution;
            _projectTitle = projectTitle;
			_projectGuid = projectGuid;
        }

        public string ProjectTitle { get { return _projectTitle; } }
        public Guid ProjectGuid { get { return _projectGuid; } }

        public IAssembly Resolve(ITypeResolveContext context)
        {
            var project = _solution.Projects.FirstOrDefault(p => p.ProjectId == _projectGuid);
            if (project != null) 
                return project.ProjectContent.Resolve(context);
            return null;
        }
    }
}
