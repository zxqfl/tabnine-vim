using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Microsoft.Build.Evaluation
{
    public class ProjectItem
    {
        private XElement Element;
        public ProjectItem(XElement element)
        {
            Element = element;
        }
        public string EvaluatedInclude
        {
            get
            {
                string value = this.Element.Attribute("Include").Value;
                if(value == null)
                    return null;

                return System.Uri.UnescapeDataString(value);
            }
        }
        public bool HasMetadata(string name)
        {
            return Element.Descendants(Element.Document.Root.Name.Namespace + name).Any();
        }
        public string GetMetadataValue(string name)
        {
            return Element.Descendants(Element.Document.Root.Name.Namespace + name).First().Value;
        }
    }
}
