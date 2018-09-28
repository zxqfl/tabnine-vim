using System.Linq;
using NUnit.Framework;
using Nancy.Testing;
using OmniSharp.Common;
using OmniSharp.Solution;
using Should;
using OmniSharp.FindUsages;

namespace OmniSharp.Tests.FindUsages
{
    [TestFixture]
    public class IntegrationTest
    {
        [Test]
        public void Should_find_usages_of_class()
        {
            const string editorText = 
@"public class myclass
{
    public void method() { }

    public void method_calling_method()
    {
        method();        
    }
}
";
            var solution = new FakeSolution();
            var project = new FakeProject();
            project.AddFile(editorText);
            solution.Projects.Add(project);

            var handler = new FindUsagesHandler (new OmniSharp.Parser.BufferParser (solution), solution, new ProjectFinder(solution));
            var usages = handler.FindUsages (new FindUsagesRequest { 
                FileName = "myfile",
                Line = 3,
                Column = 21,
                Buffer = editorText
            }).QuickFixes.ToArray();

            usages.Length.ShouldEqual(2);
            usages[0].Text.Trim().ShouldEqual("public void method() { }");
            usages[1].Text.Trim().ShouldEqual("method();");
        }
    }
}
