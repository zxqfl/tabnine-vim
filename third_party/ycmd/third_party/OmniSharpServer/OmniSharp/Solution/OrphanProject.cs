using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;
using System.Linq;

namespace OmniSharp.Solution
{
    /// <summary>
    /// Placeholder that can be used for files that don't belong to a project yet.
    /// </summary>
    public class OrphanProject : Project
    {
        public const string ProjectFileName = "OrphanProject";
        public OrphanProject(IFileSystem fileSystem, Logger logger) : base(fileSystem, logger)
        {
            Title = "Orphan Project";
            Files = new List<CSharpFile>();

            ProjectId = Guid.NewGuid();

            References = new List<IAssemblyReference>();
            FileName = ProjectFileName;
            string mscorlib = FindAssembly("mscorlib");
            Console.WriteLine(mscorlib);
            ProjectContent = new CSharpProjectContent()
                .SetAssemblyName(ProjectFileName)
                .AddAssemblyReferences(LoadAssembly(mscorlib));
        }

        private CSharpFile GetFile(string fileName, string source)
        {
            var file = Files.FirstOrDefault(f => f.FileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase));
            if (file == null)
            {
                file = new CSharpFile(this, fileName, source);
                Files.Add (file);

                this.ProjectContent = this.ProjectContent
                    .AddOrUpdateFiles(file.ParsedFile);
            }
            return file;
        }
    }
}
