using System;
using OmniSharp.Parser;
using OmniSharp.TypeLookup;
using OmniSharp.Configuration;

namespace OmniSharp.Tests.TypeLookup
{
    public static class StringExtensions
    {
        public static string LookupType(this string editorText)
        {
            int cursorOffset = editorText.IndexOf("$", StringComparison.Ordinal);
            var cursorPosition = TestHelpers.GetLineAndColumnFromIndex(editorText, cursorOffset);
            editorText = editorText.Replace("$", "");

            var solution = new FakeSolution();
            var project = new FakeProject();
            project.AddFile(editorText);
            solution.Projects.Add(project);

            var handler = new TypeLookupHandler(solution, new BufferParser(solution), new OmniSharpConfiguration());
            var request = new TypeLookupRequest()
            {
                Buffer = editorText,
                FileName = "myfile",
                Line = cursorPosition.Line,
                Column = cursorPosition.Column,
            };

            return handler.GetTypeLookupResponse(request).Type;
        }
    }
}
