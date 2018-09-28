using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using OmniSharp.Solution;
using OmniSharp.Common;

namespace OmniSharp.ProjectManipulation.AddToProject
{
    public class AddToProjectHandler
    {
        private readonly ISolution _solution;
        private readonly XNamespace _msBuildNameSpace = "http://schemas.microsoft.com/developer/msbuild/2003";

        public AddToProjectHandler(ISolution solution)
        {
            _solution = solution;
        }

        public void AddToProject(AddToProjectRequest request)
        {
            if (request.FileName == null || !request.FileName.EndsWith(".cs"))
            {
                return;
            }

            var relativeProject = _solution.ProjectContainingFile(request.FileName);

            if (relativeProject == null || relativeProject is OrphanProject)
            {
                throw new ProjectNotFoundException(string.Format("Unable to find project relative to file {0}", request.FileName));
            }

            var project = relativeProject.AsXml();

            var relativeFileName =
                new Uri(relativeProject.FileName)
                .MakeRelativeUri(new Uri(request.FileName))
                .ToString()
                .ForceWindowsPathSeparator();

            var absoluteFileName = request.FileName.ForceWindowsPathSeparator();

            var compilationNodes = project.Element(_msBuildNameSpace + "Project")
                                          .Elements(_msBuildNameSpace + "ItemGroup")
                                          .Elements(_msBuildNameSpace + "Compile").ToList();

            var fileAlreadyInProject = compilationNodes
                .Select(n => n.Attribute("Include").Value)
                .Any(path =>
                    path.Equals(absoluteFileName, StringComparison.InvariantCultureIgnoreCase) ||
                    path.Equals(relativeFileName, StringComparison.InvariantCultureIgnoreCase));


            if (!fileAlreadyInProject)
            {
                var compilationNodeParent = compilationNodes.First().Parent;

                var newFileElement = new XElement(_msBuildNameSpace + "Compile", new XAttribute("Include", relativeFileName));

                compilationNodeParent.Add(newFileElement);
                
                relativeProject.Save(project);
            }
        }
    }
}