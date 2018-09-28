using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;
using OmniSharp.Solution;
using System.Linq;

namespace OmniSharp.Tests
{
    public class FakeProject : Project
    {
        static readonly Lazy<IUnresolvedAssembly> mscorlib
            = new Lazy<IUnresolvedAssembly>(() => new CecilLoader().LoadAssemblyFile(typeof(object).Assembly.Location));

        static readonly Lazy<IUnresolvedAssembly> systemCore
            = new Lazy<IUnresolvedAssembly>(() => new CecilLoader().LoadAssemblyFile(typeof(Enumerable).Assembly.Location));



        public FakeProject(string name = "fake", string fileName = "fake.csproj", Guid id = new Guid())
            : base(new MockFileSystem(), new Logger(Verbosity.Quiet))
        {
            Name = name;
            FileName = fileName;
            Files = new List<CSharpFile>();
            References = new List<IAssemblyReference>();
            ProjectId = id;
            this.ProjectContent
                             = new CSharpProjectContent()
                             .SetAssemblyName(name)
                             .SetProjectFileName(name)
                             .AddAssemblyReferences(new [] { mscorlib.Value, systemCore.Value });
        }

        public void AddFile(string source, string fileName = "myfile")
        {
            Files.Add(new CSharpFile(this, fileName, source));    
            this.ProjectContent = this.ProjectContent
             .AddOrUpdateFiles(Files.Select(f => f.ParsedFile));
        }
    }
}