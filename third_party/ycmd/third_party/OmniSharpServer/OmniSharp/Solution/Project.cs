using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Xml.Linq;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.Utils;
using ICSharpCode.NRefactory.Editor;

namespace OmniSharp.Solution
{
    public class Project : IProject
    {
        Logger _logger;
        readonly IFileSystem _fileSystem;

        public Project (IFileSystem fileSystem, Logger logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;
            Files = new List<CSharpFile> ();
            References = new List<IAssemblyReference>();
            ProjectContent = new CSharpProjectContent();
        }

        public CompilerSettings CompilerSettings { get; set; }

        static ConcurrentDictionary<string, IUnresolvedAssembly> assemblyDict = new ConcurrentDictionary<string, IUnresolvedAssembly> (Platform.FileNameComparer);

        public string Name { get; set; }

        public void UpdateFile(string fileName, string source)
        {
            var file = GetFile (fileName, source);
            file.Content = new StringTextSource (source);
            file.Parse (this, fileName, source);

            this.ProjectContent = this.ProjectContent
                .AddOrUpdateFiles (file.ParsedFile);
        }

        private CSharpFile GetFile(string fileName, string source)
        {
            var file = Files.FirstOrDefault (f => f.FileName.Equals (fileName, StringComparison.InvariantCultureIgnoreCase));
            if (file == null)
            {
                file = new CSharpFile (this, fileName, source);
                Files.Add (file);

                this.ProjectContent = this.ProjectContent
                    .AddOrUpdateFiles (file.ParsedFile);
            }
            return file;
        }

        public CSharpParser CreateParser()
        {
            return new CSharpParser (CompilerSettings);
        }

        public XDocument AsXml()
        {
            var xml = _fileSystem.File.ReadAllText (FileName);
            return XDocument.Parse (xml);
        }

        public void AddReference(IAssemblyReference reference)
        {
            References.Add (reference);
            ProjectContent = ProjectContent.AddAssemblyReferences (References);
        }

        public void AddReference(string reference)
        {
            try
            {
                References.Add (LoadAssembly (reference));
                ProjectContent = ProjectContent.AddAssemblyReferences (References);
            }
            catch (BadImageFormatException)
            {
                // Ignore native dlls
                _logger.Error (reference + " is a native dll");
            }
        }

        public virtual IUnresolvedAssembly LoadAssembly(string assemblyFileName)
        {
            if (!_fileSystem.File.Exists (assemblyFileName))
            {
                throw new FileNotFoundException ("Assembly does not exist!", assemblyFileName);
            }

            return assemblyDict.GetOrAdd (assemblyFileName, file => new CecilLoader ().LoadAssemblyFile (file));
        }

        public void Save(XDocument project)
        {
            _fileSystem.File.WriteAllText (FileName, project.ToString ());
        }

        public string FindAssembly(string evaluatedInclude)
        {
            if (evaluatedInclude.IndexOf (',') >= 0)
                evaluatedInclude = evaluatedInclude.Substring (0, evaluatedInclude.IndexOf (','));

            string directAssemblyFile = (evaluatedInclude + ".dll").ForcePathSeparator(Path.DirectorySeparatorChar);
            if (_fileSystem.File.Exists (directAssemblyFile))
                return directAssemblyFile;

            foreach (string searchPath in AssemblySearch.Paths)
            {
                string assemblyFile = _fileSystem.Path.Combine (searchPath, evaluatedInclude + ".dll");
                if (_fileSystem.File.Exists (assemblyFile))
                    return assemblyFile;
            }
            return null;
        }

        public IProjectContent ProjectContent { get; set; }

        public string Title { get; set; }

        public string FileName { get; set; }

        public List<CSharpFile> Files { get; protected set; }

        public List<IAssemblyReference> References { get; set; }

        public Guid ProjectId { get; set; }
    }
    
}
