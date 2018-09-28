using System.Collections.Generic;

namespace OmniSharp.FindProjects
{
    public class MsBuildWorkspaceInformation
    {
        public string SolutionPath { get; set; }
        public IEnumerable<MsBuildProject> Projects { get; set; }
    }
}