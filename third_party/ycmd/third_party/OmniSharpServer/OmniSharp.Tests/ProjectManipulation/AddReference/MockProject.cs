using System;
using System.IO.Abstractions;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using OmniSharp.Solution;

namespace OmniSharp.Tests.ProjectManipulation.AddReference
{
    public class MockProject : CSharpProject
    {
        public MockProject (ISolution solution, IFileSystem fileSystem, Logger logger, string fileName) 
            : base(solution, fileSystem, logger, "name", fileName, Guid.NewGuid())
        {
            
        }

        public override IUnresolvedAssembly LoadAssembly(string assemblyFileName)
        {
            return new DefaultUnresolvedAssembly(assemblyFileName);
        }
    }
    
}