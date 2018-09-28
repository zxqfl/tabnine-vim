using System.Linq;
using System.Xml.Linq;
using OmniSharp.Solution;

namespace OmniSharp.ProjectManipulation.AddReference
{
    public class AddGacReferenceProcessor : ReferenceProcessorBase, IReferenceProcessor
    {
        public AddReferenceResponse AddReference(IProject project, string reference)
        {
            var response = new AddReferenceResponse();

            var projectXml = project.AsXml();

            var referenceNodes = GetReferenceNodes(projectXml, "Reference");

            var referenceAlreadyAdded = referenceNodes.Any(n => n.Attribute("Include").Value.Equals(reference));


            if (!referenceAlreadyAdded)
            {
                var fileReferenceNode = CreateReferenceNode(reference);
                if (referenceNodes.Count > 0)
                {
                    referenceNodes.First().Parent.Add(fileReferenceNode);
                }
                else
                {
                    var projectItemGroup = new XElement(MsBuildNameSpace + "ItemGroup");
                    projectItemGroup.Add(fileReferenceNode);
                    projectXml.Element(MsBuildNameSpace + "Project").Add(projectItemGroup);
                }

                var assemblyPath = project.FindAssembly (reference);
                if (assemblyPath != null)
                {
                    project.AddReference (assemblyPath);
                    project.Save (projectXml);
                    response.Message = string.Format ("Reference to {0} added successfully", reference);
                }
                else
                {
                    response.Message = "Did not find " + reference;
                }
            }
            else
            {
                response.Message = "Reference already added";
            }

            return response;
        }

        XElement CreateReferenceNode(string referenceName)
        {
            var projectReferenceNode =
                new XElement(MsBuildNameSpace + "Reference",
                    new XAttribute("Include", referenceName));

            return projectReferenceNode;
        }
    }
}
