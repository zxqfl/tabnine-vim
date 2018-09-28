using System;
using OmniSharp.Solution;

namespace OmniSharp.FindProjects
{
    public class MsBuildProject
    {
        public Guid ProjectGuid { get; set; }
        public string Path { get; set; }
        public string AssemblyName { get; set; }

        public MsBuildProject(IProject p)
        {
            AssemblyName = p.Title;
            Path = p.FileName;
            ProjectGuid = p.ProjectId;
        }
    }
}