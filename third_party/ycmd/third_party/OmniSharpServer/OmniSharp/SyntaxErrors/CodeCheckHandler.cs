using System.Collections.Generic;
using System.Linq;
using OmniSharp.CodeIssues;
using OmniSharp.Common;
using OmniSharp.SemanticErrors;

namespace OmniSharp.SyntaxErrors
{
    public class CodeCheckHandler
    {
        readonly SyntaxErrorsHandler _syntaxErrorsHandler;
        readonly CodeIssuesHandler _codeIssuesHandler;
        readonly SemanticErrorsHandler _semanticErrorsHandler;

        public CodeCheckHandler(SyntaxErrorsHandler syntaxErrorsHandler, CodeIssuesHandler codeIssuesHandler, SemanticErrorsHandler semanticErrorsHandler)
        {
            _semanticErrorsHandler = semanticErrorsHandler;
            _codeIssuesHandler = codeIssuesHandler;
            _syntaxErrorsHandler = syntaxErrorsHandler;
        }

        public IEnumerable<QuickFix> CodeCheck(Request request)
        {
            var errors = new List<QuickFix>();

            var syntaxErrors =
                _syntaxErrorsHandler.FindSyntaxErrors(request)
                    .Errors.Select(
                        x => new QuickFix {
                            FileName = x.FileName,
                            Column = x.Column, 
                            Line = x.Line,
                            EndColumn = x.EndColumn,
                            EndLine = x.EndLine,
                            Text = x.Message, 
                            LogLevel = "Error"});

            errors.AddRange(syntaxErrors);

            if (errors.Any())
            {
                return errors;
            }

            var semanticErrors =
                _semanticErrorsHandler.FindSemanticErrors(request)
                    .Errors.Select(
                        x => new QuickFix {
                            Column = x.Column, 
                            FileName = x.FileName, 
                            Line = x.Line, 
                            Text = x.Message , 
                            LogLevel = "Error"});

            errors.AddRange(semanticErrors);

            if (errors.Any())
            {
                return errors;
            }

            var codeErrors = _codeIssuesHandler.GetCodeIssues(request).QuickFixes;
            errors.AddRange(codeErrors);

            return errors;
        }
    }
}
