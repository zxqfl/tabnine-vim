using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ICSharpCode.NRefactory;
using OmniSharp.AutoComplete;
using OmniSharp.Parser;
using OmniSharp.Configuration;

namespace OmniSharp.Tests.AutoComplete
{
    public class CompletionsSpecBase
    {
        private readonly FakeSolution _solution;

        public CompletionsSpecBase()
        {
            _solution = new FakeSolution();
        }

        public IEnumerable<CompletionData> GetCompletions(string editorText, bool includeImportableTypes)
        {
            int cursorOffset = editorText.IndexOf("$", StringComparison.Ordinal);
            if (cursorOffset == -1)
                throw new ArgumentException("Editor text should contain a $");

            TextLocation cursorPosition = TestHelpers.GetLineAndColumnFromIndex(editorText, cursorOffset);
            string partialWord = GetPartialWord(editorText);
            editorText = editorText.Replace("$", "");

            var project = new FakeProject();
            project.AddFile(editorText);
            _solution.Projects.Add(project);
            var provider = new AutoCompleteHandler(_solution, new BufferParser(_solution), new Logger(Verbosity.Quiet), new OmniSharpConfiguration());
            var request = new AutoCompleteRequest
            {
                FileName = "myfile",
                WordToComplete = partialWord,
                Buffer = editorText,
                Line = cursorPosition.Line,
                Column = cursorPosition.Column,
                WantDocumentationForEveryCompletionResult = false,
                WantImportableTypes = includeImportableTypes,
                WantMethodHeader = true,
                WantReturnType = true,
                WantSnippet = true
            };

            return provider.CreateProvider(request);
        }

        private static string GetPartialWord(string editorText)
        {
            MatchCollection matches = Regex.Matches(editorText, @"([a-zA-Z0-9_]*)\$");
            return matches[0].Groups[1].ToString();
        }
    }
}
