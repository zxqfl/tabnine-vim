using System.IO;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using OmniSharp.AutoComplete;
using OmniSharp.Configuration;
using OmniSharp.Parser;
using OmniSharp.Solution;

namespace OmniSharp.TestContextInformation
{
    public class GetTestContextHandler
    {
        private readonly ISolution _solution;
        private readonly BufferParser _parser;

        public GetTestContextHandler(ISolution solution, BufferParser parser)
        {
            _solution = solution;
            _parser = parser;
        }

        public GetTestContextResponse GetTestContextResponse(TestCommandRequest request)
        {

            var contextInfo = GetContextResponse(request);
            var testCommand = TestCommand(request, contextInfo.AssemblyName, contextInfo.TypeName, contextInfo.MethodName);

            return new GetTestContextResponse
                {
                    TestCommand = testCommand
                };
        }

        public GetContextResponse GetContextResponse(TestCommandRequest request)
        {
            string methodName = null;

            var bufferContext = new BufferContext(request, _parser);
			var node = bufferContext.NodeCurrentlyUnderCursor;

            TypeDeclaration type = null;
			NamespaceDeclaration namespaceDeclaration = null;
			if(node != null)
			{
				var method = (MethodDeclaration) node.AncestorsAndSelf.FirstOrDefault(n => n is MethodDeclaration);
				if (method != null)
					methodName = method.Name;
				type = (TypeDeclaration)node.AncestorsAndSelf.FirstOrDefault(n => n is TypeDeclaration);
				namespaceDeclaration = (NamespaceDeclaration)node.AncestorsAndSelf.FirstOrDefault(n => n is NamespaceDeclaration);
			}

            if (type == null)
			{
				var tree = bufferContext.ParsedContent.SyntaxTree;
				type = (TypeDeclaration)tree.DescendantsAndSelf.FirstOrDefault(n => n is TypeDeclaration);
				namespaceDeclaration = (NamespaceDeclaration)tree.DescendantsAndSelf.FirstOrDefault(n => n is NamespaceDeclaration);
			}

			string typeName = type.Name;

            if (namespaceDeclaration != null)
                typeName = namespaceDeclaration.FullName + "." + typeName;

            var project = _solution.ProjectContainingFile(request.FileName);
            var directory = new FileInfo(project.FileName).Directory.FullName;

            var assemblyName = "\"" +
                               Path.Combine(directory, "bin", "Debug", project.ProjectContent.FullAssemblyName + ".dll")
                               + "\"";

            return new GetContextResponse
            {
                AssemblyName = assemblyName,
                TypeName = typeName,
                MethodName = methodName
            };

        }
        private static string TestCommand(TestCommandRequest request, string assemblyName, string typeName, string methodName)
        {
            var testCommands = ConfigurationLoader.Config.TestCommands;
            string testCommand = testCommands.All;
            switch (request.Type)
            {
                case TestCommandRequest.TestCommandType.All:
                    testCommand = testCommands.All;
                    break;
                case TestCommandRequest.TestCommandType.Fixture:
                    testCommand = testCommands.Fixture;
                    break;
                case TestCommandRequest.TestCommandType.Single:
                    testCommand = testCommands.Single;
                    break;
            }

            testCommand = testCommand.Replace("{{AssemblyPath}}", assemblyName)
                .Replace("{{TypeName}}", typeName)
                .Replace("{{MethodName}}", methodName);
            
            return testCommand;
        }
    }
}
