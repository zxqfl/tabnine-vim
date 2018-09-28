using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using OmniSharp.ProjectManipulation;
using OmniSharp.ProjectManipulation.RemoveFromProject;
using OmniSharp.Solution;
using Should;
using OmniSharp.Tests.ProjectManipulation.AddReference;

namespace OmniSharp.Tests.ProjectManipulation.RemoveFromProject
{
    [TestFixture]
    public class RemoveFromProjectTests
    {
        ISolution Solution;
        MockFileSystem _fs;

        [SetUp]
        public void SetUp()
        {
            Solution = new FakeSolution(@"c:\test\fake.sln");
            _fs = new MockFileSystem();
        }

        IProject GetProject(string content, string projFileName = @"c:\test\code\fake1.csproj")
        {
            _fs.File.WriteAllText(projFileName, content);
            var project = new MockProject(Solution, _fs, new Logger(Verbosity.Quiet), projFileName);
            project.FileName = projFileName;
            project.Files.Add(new CSharpFile(project, @"c:\test\code\test.cs", "some c# code"));
            return project;
        }

        [Test, ExpectedException(typeof(ProjectNotFoundException))]
        public void ShouldThrowProjectNotFoundExceptionWhenProjectNotFound()
        {
            var project = GetProject("<xml/>", @"/test/code/fake.csproj");
            var solution = new FakeSolution(@"/test/fake.sln");
            solution.Projects.Add(project);

            var request = new RemoveFromProjectRequest
            {
                FileName = @"/test/folder/Test.cs"
            };

            var handler = new RemoveFromProjectHandler(solution);
            handler.RemoveFromProject(request);
        }

        [Test]
        public void ShouldRemoveFileFromProjectXml()
        {

            var xml = @"
               <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                   <ItemGroup>
                       <Compile Include=""Hello.cs""/>
                       <Compile Include=""Test.cs""/>
                   </ItemGroup>
               </Project>";
            var project = GetProject(xml, @"c:\test\code\fake1.csproj");

            var expectedXml = XDocument.Parse(@"
               <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                   <ItemGroup>
                       <Compile Include=""Hello.cs""/>
                   </ItemGroup>
               </Project>");

            var solution = new FakeSolution(@"c:\test\fake.sln");
            solution.Projects.Add(project);

            var request = new RemoveFromProjectRequest
            {
                FileName = @"c:\test\code\test.cs"
            };

            var handler = new RemoveFromProjectHandler(solution);
            handler.RemoveFromProject(request);

            project.AsXml().ToString().ShouldEqual(expectedXml.ToString());
        }

        [Test]
        public void ShouldRemoveItemGroupWhenRemovingLastFile()
        {
            var xml = @"
               <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                   <ItemGroup>
                       <Compile Include=""Test.cs""/>
                   </ItemGroup>
               </Project>";
            var project = GetProject(xml);


            var solution = new FakeSolution(@"c:\test\fake.sln");
            solution.Projects.Add(project);

            var request = new RemoveFromProjectRequest
            {
                FileName = @"c:\test\code\test.cs"
            };

            var handler = new RemoveFromProjectHandler(solution);
            handler.RemoveFromProject(request);

            XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";
            project.AsXml().Descendants(ns + "ItemGroup").Count().ShouldEqual(0);
        }

        [Test]
        public void ShouldRemoveFileFromProject()
        {
            var xml = @"
               <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                   <ItemGroup>
                       <Compile Include=""Test.cs""/>
                   </ItemGroup>
               </Project>";

            var project = GetProject(xml);

            var solution = new FakeSolution(@"c:\test\fake.sln");
            solution.Projects.Add(project);

            var request = new RemoveFromProjectRequest
            {
                FileName = @"c:\test\code\test.cs"
            };

            var handler = new RemoveFromProjectHandler(solution);
            handler.RemoveFromProject(request);

            project.Files.ShouldBeEmpty();
        }
    }
}
