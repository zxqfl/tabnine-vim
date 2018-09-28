using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.IO.Abstractions;

namespace Microsoft.Build.Evaluation
{
    public class Project
    {

        public string DirectoryPath { get; private set; }

        readonly XDocument document;

        public Project(IFileSystem fileSystem, string fileName)
        {
            DirectoryPath = fileSystem.Path.GetDirectoryName(fileName);
            var xml = fileSystem.File.ReadAllText(fileName);
            document = XDocument.Parse(xml);
        }

        public string GetPropertyValue(string name)
        {
            XElement element = document.Descendants(document.Root.Name.Namespace + "PropertyGroup").Descendants(document.Root.Name.Namespace + name).FirstOrDefault();
            return element == null ? string.Empty : element.Value;
        }

        public ICollection<ProjectItem> GetItems(string itemType)
        {
            IEnumerable<XElement> elements = document.Descendants(document.Root.Name.Namespace + "ItemGroup").Descendants(document.Root.Name.Namespace + itemType);
            return (from element in elements select new ProjectItem(element)).ToList();
        }
    }
}
