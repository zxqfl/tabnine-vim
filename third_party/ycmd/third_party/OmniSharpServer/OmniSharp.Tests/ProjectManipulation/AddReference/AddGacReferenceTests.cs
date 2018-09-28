using NUnit.Framework;
using OmniSharp.Common;
using OmniSharp.ProjectManipulation.AddReference;
using OmniSharp.Solution;
using System.IO.Abstractions.TestingHelpers;

namespace OmniSharp.Tests.ProjectManipulation.AddReference
{
    [TestFixture]
    public class AddGacReferenceTests
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
         public void CanAddGacAssemblyReference()
         {
            var project = GetProject(
                @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                    <ItemGroup>
                        <Compile Include=""Test.cs""/>
                    </ItemGroup>
                </Project>");
             Solution.Projects.Add(project);

            var path = 
                _fs.Path.Combine(@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5", "System.Web.Mvc.dll");
            _fs.File.WriteAllText(path, "");
             const string expectedXml = @"
                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                    <ItemGroup>
                        <Compile Include=""Test.cs""/>
                    </ItemGroup>
                    <ItemGroup>
                        <Reference Include=""System.Web.Mvc"" />
                    </ItemGroup>
                </Project>";

             var request = new AddReferenceRequest
             {
                 Reference = @"System.Web.Mvc",
                 FileName = @"c:\test\one\test.cs"
             };

             var handler = new AddReferenceHandler(Solution, new AddReferenceProcessorFactory(Solution, new IReferenceProcessor[]{new AddGacReferenceProcessor() }, new NativeFileSystem()));
             var response = handler.AddReference(request);
            response.Message.ShouldEqual("Reference to System.Web.Mvc added successfully");
            _fs.File.ReadAllText(project.FileName).ShouldEqualXml(expectedXml);
         }

        [Test]
        public void ShouldNotAddGacReferenceThatDoesNotExist()
        {
            const string xml = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                    <ItemGroup>
                        <Compile Include=""Test.cs""/>
                    </ItemGroup>
                </Project>";

            var project = GetProject(xml);
            Solution.Projects.Add(project);

            var request = new AddReferenceRequest
            {
                Reference = @"System.Web.Mvc",
                FileName = @"c:\test\one\test.cs"
             };

            var handler = new AddReferenceHandler(Solution, new AddReferenceProcessorFactory(Solution, new IReferenceProcessor[]{new AddGacReferenceProcessor() }, new NativeFileSystem()));
            var response = handler.AddReference(request);
            response.Message.ShouldEqual("Did not find System.Web.Mvc");
            _fs.File.ReadAllText(project.FileName).ShouldEqualXml(xml);
        }

        [Test]
        public void WillNotAddDuplicateGacAssemblyReference()
        {
            var project = GetProject(
                @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                    <ItemGroup>
                        <Compile Include=""Test.cs""/>
                    </ItemGroup>
                    <ItemGroup>
                        <Reference Include=""System.Web.Mvc"" />
                    </ItemGroup>
                </Project>");

            Solution.Projects.Add(project);

            const string expectedXml = @"
                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                    <ItemGroup>
                        <Compile Include=""Test.cs""/>
                    </ItemGroup>
                    <ItemGroup>
                        <Reference Include=""System.Web.Mvc"" />
                    </ItemGroup>
                </Project>";

            var request = new AddReferenceRequest
            {
                Reference = @"System.Web.Mvc",
                FileName = @"c:\test\one\test.cs"
            };

            var handler = new AddReferenceHandler(Solution, new AddReferenceProcessorFactory(Solution, new IReferenceProcessor[] { new AddGacReferenceProcessor() }, new NativeFileSystem()));
            handler.AddReference(request);
            _fs.File.ReadAllText(project.FileName).ShouldEqualXml(expectedXml);
        }
    }
}