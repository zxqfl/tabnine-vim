using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Xml.Linq;
using NUnit.Framework;
using OmniSharp.ProjectManipulation;
using OmniSharp.ProjectManipulation.AddToProject;
using OmniSharp.Solution;
using Should;
using OmniSharp.Tests.ProjectManipulation.AddReference;

namespace OmniSharp.Tests.ProjectManipulation.AddToProject
{
    [TestFixture]
    public class AddToProjectTests
    {
        ISolution Solution;
        MockFileSystem _fs;

        [SetUp]
        public void SetUp()
        {
            Solution = new FakeSolution (@"c:\test\fake.sln");
            _fs = new MockFileSystem ();
        }

        IProject GetProject(string content, string projFileName = @"c:\test\code\fake1.csproj")
        {
            _fs.File.WriteAllText (projFileName, content);
            var project = new MockProject (Solution, _fs, new Logger (Verbosity.Quiet), projFileName);
            project.FileName = projFileName;
            project.Files.Add (new CSharpFile (project, @"c:\test\code\test.cs", "some c# code"));
            project.Files.Add (new CSharpFile (project, @"c:\test2\Absolute.cs", "some c# code"));
            project.Files.Add (new CSharpFile (project, @"c:\test2\Foreign.cs", "some c# code"));
            return project;
        }

        [Test]
        public void ShouldNotAddFileToProjectWhenAlreadyExists()
        {
            var project = GetProject (@"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003""><ItemGroup><Compile Include=""Hello.cs""/><Compile Include=""Test.cs""/><Compile Include=""c:\test2\Absolute.cs""/><Compile Include=""..\..\test2\Foreign.cs""/></ItemGroup></Project>");
            var expectedXml = project.AsXml ();

            var solution = new FakeSolution (@"c:\test\fake.sln");
            solution.Projects.Add (project);

            // test relative internal

            var request = new AddToProjectRequest {
                FileName = @"c:\test\code\test.cs"
            };

            var handler = new AddToProjectHandler (solution);
            handler.AddToProject (request);
            project.AsXml ().ToString ().ShouldEqual (expectedXml.ToString ());

            // test absolute
            request = new AddToProjectRequest {
                FileName = @"c:\test2\Absolute.cs"
            };

            handler = new AddToProjectHandler (solution);
            handler.AddToProject (request);
            project.AsXml ().ToString ().ShouldEqual (expectedXml.ToString ());

            // test relative foreign tree

            request = new AddToProjectRequest {
                FileName = @"c:\test2\Foreign.cs"
            };

            handler = new AddToProjectHandler (solution);
            handler.AddToProject (request);
            project.AsXml ().ToString ().ShouldEqual (expectedXml.ToString ());
        }

        [Test]
        public void ShouldAddNewFileToProject()
        {
            var project = GetProject (@"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003""><ItemGroup><Compile Include=""Hello.cs""/></ItemGroup></Project>");

            project.Files.Add (new CSharpFile (project, @"c:\test\code\files\Test.cs", "some c# code"));
           
            var solution = new FakeSolution (@"c:\test\fake.sln");
            solution.Projects.Add (project);

            var request = new AddToProjectRequest {
                FileName = @"c:\test\code\files\Test.cs"
            };

            var handler = new AddToProjectHandler (solution);
            handler.AddToProject (request);

            var expectedXml = XDocument.Parse (
                                  @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003""><ItemGroup><Compile Include=""Hello.cs""/><Compile Include=""files\Test.cs""/></ItemGroup></Project>");

            project.AsXml ().ToString ().ShouldEqual (expectedXml.ToString ());
        }

        [Test]
        public void ShouldNotAddNonCSharpFile()
        {
            var project = GetProject (@"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003""><ItemGroup><Compile Include=""Hello.cs""/></ItemGroup></Project>");

            var expectedXml = XDocument.Parse (@"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003""><ItemGroup><Compile Include=""Hello.cs""/></ItemGroup></Project>");

            // project.AddFile("some content", @"c:\test\code\foo.txt");

            var solution = new FakeSolution (@"c:\test\fake.sln");
            solution.Projects.Add (project);

            var request = new AddToProjectRequest {
                FileName = @"c:\test\code\foo.txt"
            };

            var handler = new AddToProjectHandler (solution);
            handler.AddToProject (request);

            project.AsXml ().ToString ().ShouldEqual (expectedXml.ToString ());
        }

        [Test]
        public void ShouldAlwaysUseWindowsFileSeparatorWhenAddingToProject()
        {
            if (PlatformService.IsUnix)
            {
                var project = GetProject(@"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003""><ItemGroup><Compile Include=""Hello.cs""/></ItemGroup></Project>"
                    ,@"/test/code/fake.csproj" );
                var expectedXml = XDocument.Parse (@"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003""><ItemGroup><Compile Include=""Hello.cs""/><Compile Include=""folder\Test.cs""/></ItemGroup></Project>");
                project.Files.Add (new CSharpFile (project, @"/test/code/folder/Test.cs", "some c# code"));

                var solution = new FakeSolution (@"/test/fake.sln");
                solution.Projects.Add (project);

                var request = new AddToProjectRequest {
                    FileName = @"/test/code/folder/Test.cs"
                };

                var handler = new AddToProjectHandler (solution);
                handler.AddToProject (request);

                project.AsXml().ToString().ShouldEqual (expectedXml.ToString ());
            }
        }

        [Test, ExpectedException (typeof(ProjectNotFoundException))]
        public void ShouldThrowProjectNotFoundExceptionWhenProjectNotFound()
        {
            var project = GetProject ("<xml/>");
            var solution = new FakeSolution (@"/test/fake.sln");
            solution.Projects.Add (project);

            var request = new AddToProjectRequest {
                FileName = @"/test/folder/Test.cs"
            };

            var handler = new AddToProjectHandler (solution);
            handler.AddToProject (request);
        }

        [Test, ExpectedException (typeof(ProjectNotFoundException))]
        public void ShouldThrowProjectNotFoundExceptionForOrphanProject()
        {
            var solution = new FakeSolution (@"/test/fake.sln");
            var project = new OrphanProject (new FileSystem (), new Logger (Verbosity.Quiet));
            project.Files.Add (new CSharpFile (project, "/test/folder/Test.cs", "Some content..."));
            solution.Projects.Add (project);

            var request = new AddToProjectRequest {
                FileName = @"/test/folder/Test.cs"
            };

            var handler = new AddToProjectHandler (solution);
            handler.AddToProject (request);
        }
    }
}