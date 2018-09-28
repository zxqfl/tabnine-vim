using System;
using System.IO;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.TypeSystem;

namespace OmniSharp.Solution
{
    public class CSharpFile
    {
        public string FileName;

        public ITextSource Content;
        public SyntaxTree SyntaxTree;
        public IUnresolvedFile ParsedFile;


        public StringBuilderDocument Document { get; set; }

        public CSharpFile(IProject project, string fileName) : this(project, fileName, File.ReadAllText(fileName))
        {
        }

        public CSharpFile(IProject project, string fileName, string source)
        {
            Parse(project, fileName, source);
        }

        public void Parse(IProject project, string fileName, string source)
        {
            this.FileName = fileName;
            this.Content = new StringTextSource(source);
            this.Document = new StringBuilderDocument(this.Content);
            this.Project = project;
            CSharpParser p = project.CreateParser();
            this.SyntaxTree = p.Parse(Content.CreateReader(), fileName);
            this.ParsedFile = this.SyntaxTree.ToTypeSystem();
        }

        protected IProject Project { get; set; }


    }
}
