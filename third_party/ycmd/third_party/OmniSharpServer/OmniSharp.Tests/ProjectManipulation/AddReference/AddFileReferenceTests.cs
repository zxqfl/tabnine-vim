using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using NUnit.Framework;
using OmniSharp.ProjectManipulation.AddReference;
using OmniSharp.Solution;

namespace OmniSharp.Tests.ProjectManipulation.AddReference
{
    [TestFixture]
    public class AddFileReferenceTests
    {
        ISolution Solution;
        MockFileSystem _fs;

        [SetUp]
        public void SetUp()
        {
            Solution = new FakeSolution(@"c:\test\fake.sln");
            _fs = new MockFileSystem();
        }

        IProject GetProject(string content)
        {
            const string projFileName = @"c:\test\one\fake1.csproj";
            _fs.File.WriteAllText(projFileName, content);
            var project = new MockProject(Solution, _fs, new Logger(Verbosity.Quiet), projFileName);
            project.FileName = projFileName;
            project.Files.Add(new CSharpFile(project, @"c:\test\one\test.cs", "some c# code"));
            return project;
        }

        [Test]
        public void CanAddFileReference()
        {
            var project = GetProject(
                @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                    <ItemGroup>
                        <Compile Include=""Test.cs""/>
                    </ItemGroup>
                    <ItemGroup>
                        <Reference Include=""Hello.World"">
                            <HintPath>..\packages\HelloWorld\lib\net40\Does.Not.Exist.dll</HintPath>
                        </Reference>
                    </ItemGroup>
                </Project>");

            Solution.Projects.Add(project);

            var request = new AddReferenceRequest
            {
                Reference = @"c:\test\packages\SomeTest\lib\net40\Some.Test.dll",
                FileName = @"c:\test\one\test.cs"
            };

            const string expectedXml = @"
                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                    <ItemGroup>
                        <Compile Include=""Test.cs""/>
                    </ItemGroup>
                    <ItemGroup>
                        <Reference Include=""Hello.World"">
                            <HintPath>..\packages\HelloWorld\lib\net40\Does.Not.Exist.dll</HintPath>
                        </Reference>
                        <Reference Include=""Some.Test"">
                            <HintPath>..\packages\SomeTest\lib\net40\Some.Test.dll</HintPath>
                        </Reference>
                    </ItemGroup>
                </Project>";

            var handler = new AddReferenceHandler(Solution, new AddReferenceProcessorFactory(Solution, new IReferenceProcessor[] { new AddFileReferenceProcessor() }, new FakeWindowsFileSystem()));
            handler.AddReference(request);

            _fs.File.ReadAllText(project.FileName).ShouldEqualXml(expectedXml);

            project.References.Select(r => ((DefaultUnresolvedAssembly)r).AssemblyName)
                .ShouldContain(@"c:\test\packages\SomeTest\lib\net40\Some.Test.dll");
        }

        [Test]
        public void CanAddFileReferenceWhenNoReferencesExist()
        {
            var project = GetProject(
                @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                    <ItemGroup>
                        <Compile Include=""Test.cs""/>
                    </ItemGroup>
                </Project>");

            Solution.Projects.Add(project);

            var request = new AddReferenceRequest
            {
                Reference = @"c:\test\packages\SomeTest\lib\net40\Some.Test.dll",
                FileName = @"c:\test\one\test.cs"
            };

            const string expectedXml = @"
                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                    <ItemGroup>
                        <Compile Include=""Test.cs""/>
                    </ItemGroup>
                    <ItemGroup>
                        <Reference Include=""Some.Test"">
                            <HintPath>..\packages\SomeTest\lib\net40\Some.Test.dll</HintPath>
                        </Reference>
                    </ItemGroup>
                </Project>";

            var handler = new AddReferenceHandler(Solution, new AddReferenceProcessorFactory(Solution, new IReferenceProcessor[] { new AddFileReferenceProcessor() }, new FakeFileSystem()));
            handler.AddReference(request);

            _fs.File.ReadAllText(project.FileName).ShouldEqualXml(expectedXml);

            project.References.Select(r => ((DefaultUnresolvedAssembly)r).AssemblyName)
                .ShouldContain(@"c:\test\packages\SomeTest\lib\net40\Some.Test.dll");
        }

        [Test]
        public void ShouldNotAddDuplicateFileReference()
        {
            var project = GetProject(
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

            Solution.Projects.Add(project);

            var request = new AddReferenceRequest
            {
                Reference = @"c:\test\packages\HelloWorld\lib\net40\Hello.World.dll",
                FileName = @"c:\test\one\test.cs"
            };

            const string expectedXml = @"
                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                    <ItemGroup>
                        <Compile Include=""Test.cs""/>
                    </ItemGroup>
                    <ItemGroup>
                        <Reference Include=""Hello.World"">
                            <HintPath>..\packages\HelloWorld\lib\net40\Hello.World.dll</HintPath>
                        </Reference>
                    </ItemGroup>
                </Project>";

            var handler = new AddReferenceHandler(Solution, new AddReferenceProcessorFactory(Solution, new IReferenceProcessor[] { new AddFileReferenceProcessor() }, new FakeFileSystem()));
            handler.AddReference(request);
            _fs.File.ReadAllText(project.FileName).ShouldEqualXml(expectedXml);
        }
    }
}