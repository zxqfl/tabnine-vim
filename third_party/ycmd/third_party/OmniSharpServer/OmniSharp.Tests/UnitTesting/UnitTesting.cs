using System;
using OmniSharp.Parser;
using OmniSharp.TestContextInformation;

namespace OmniSharp.Tests.UnitTesting
{
    public static class CreateContextLookup
    {
        public static GetContextResponse GetContextInformation(this string editorText)
        {
            var cursorPosition = TestHelpers.GetLineAndColumnFromDollar(editorText);
            editorText = editorText.Replace("$", "");

            var solution = new FakeSolution();
            var project = new FakeProject();
            project.AddFile(editorText);
            solution.Projects.Add(project);

            var handler = new GetTestContextHandler(solution, new BufferParser(solution));
            var request = new TestCommandRequest
            {
                Buffer = editorText,
                FileName = "myfile",
                Line = cursorPosition.Line,
                Column = cursorPosition.Column,
            };

            return handler.GetContextResponse(request);
        }
    }
}
