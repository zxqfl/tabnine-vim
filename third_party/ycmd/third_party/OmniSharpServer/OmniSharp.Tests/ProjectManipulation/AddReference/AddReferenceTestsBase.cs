using System;
using System.Xml.Linq;
using NUnit.Framework;
using OmniSharp.Solution;
using System.IO.Abstractions.TestingHelpers;

namespace OmniSharp.Tests.ProjectManipulation.AddReference
{
    public abstract class AddReferenceTestsBase
    {
        protected ISolution Solution;

        MockFileSystem _fs;

        [SetUp]
        public void SetUp()
        {
            Solution = new FakeSolution(@"c:\test\fake.sln");
            _fs = new MockFileSystem();
        }

        private IProject GetProject(string content)
        {
            var projFileName = @"c:\test\one\fake1.csproj";
            _fs.File.WriteAllText(projFileName, content);
            var project = new Project(_fs, new Logger (Verbosity.Quiet));
            project.FileName = projFileName;
            project.Files.Add(new CSharpFile(project, @"c:\test\one\test.cs", "some c# code"));
            return project;
        }

        protected IProject CreateDefaultProject()
        {
            return GetProject(
            @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                <ItemGroup>
                    <Compile Include=""Test.cs""/>
                </ItemGroup>
            </Project>");
        }

        protected IProject CreateDefaultProjectWithGacReference()
        {
            return GetProject(
                @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                    <ItemGroup>
                        <Compile Include=""Test.cs""/>
                    </ItemGroup>
                    <ItemGroup>
                        <Reference Include=""Hello.World"">
                            <HintPath>..\packages\HelloWorld\lib\net40\Hello.World.dll</HintPath>
                        </Reference>
                    </ItemGroup>
                </Project>");

//            var project = new Project(_fs, new Logger (Verbosity.Quiet));
//            //project..AddFile("some content", @"c:\test\one\test.cs");
//            return project;
//            var project = new CSharpProject("fakeone", @"c:\test\one\fake1.csproj", Guid.NewGuid())
//            {
//                Title = "Project One",
//                XmlRepresentation = XDocument.Parse(@"
//                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
//                    <ItemGroup>
//                        <Compile Include=""Test.cs""/>
//                    </ItemGroup>
//                    <ItemGroup>
//                        <Reference Include=""System.Web.Mvc"" />
//                    </ItemGroup>
//                </Project>")
//            };
//            project.AddFile("some content", @"c:\test\one\test.cs");
//            return project;
        }
    }
}