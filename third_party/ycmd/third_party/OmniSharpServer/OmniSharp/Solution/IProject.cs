using System;
using System.Collections.Generic;
using System.Xml.Linq;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;

namespace OmniSharp.Solution
{
    public interface IProject
    {
        IProjectContent ProjectContent { get; set; }
        string Title { get; set; }
        string FileName { get; }
        List<CSharpFile> Files { get; }
        List<IAssemblyReference> References { get; set; }
        void UpdateFile (string filename, string source);
        CSharpParser CreateParser();
        XDocument AsXml();
        void Save(XDocument project);
        Guid ProjectId { get; set; }
        void AddReference(IAssemblyReference reference);
        void AddReference(string reference);
        CompilerSettings CompilerSettings { get; }
        string FindAssembly (string evaluatedInclude);
    }
}