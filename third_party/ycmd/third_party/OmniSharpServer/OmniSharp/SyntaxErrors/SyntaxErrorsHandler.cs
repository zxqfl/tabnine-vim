using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using OmniSharp.Common;
using OmniSharp.Solution;
using OmniSharp.Parser;
using OmniSharp.Configuration;

namespace OmniSharp.SyntaxErrors
{
    public class SyntaxErrorsHandler
    {
		private readonly ISolution _solution;
		public SyntaxErrorsHandler(ISolution solution)
		{
			_solution = solution;
		}

        public SyntaxErrorsResponse FindSyntaxErrors(Request request)
        {
            var parser = new CSharpParser ();
            var project = _solution.ProjectContainingFile(request.FileName);
            if (project.CompilerSettings != null) {
            	parser.CompilerSettings = project.CompilerSettings;
            }
            var syntaxTree = parser.Parse(request.Buffer, request.FileName);

            var filename = request.FileName.ApplyPathReplacementsForClient();

            var errors = syntaxTree.Errors.Select(error => new Error
                {
                    Message = error.Message.Replace("'", "''"),
                    Column = error.Region.BeginColumn,
                    Line = error.Region.BeginLine,
                    EndColumn = error.Region.EndColumn,
                    EndLine = error.Region.EndLine,
                    FileName = filename
                });

            return new SyntaxErrorsResponse {Errors = errors};
        }
    }
}
