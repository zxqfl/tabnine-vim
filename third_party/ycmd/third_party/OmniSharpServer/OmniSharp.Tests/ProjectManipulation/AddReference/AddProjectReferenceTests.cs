using System;
using System.Xml.Linq;
using NUnit.Framework;
using OmniSharp.Common;
using OmniSharp.ProjectManipulation.AddReference;
using OmniSharp.Solution;
using Should;
using System.IO.Abstractions.TestingHelpers;

namespace OmniSharp.Tests.ProjectManipulation.AddReference
{
    [TestFixture]
    public class AddProjectReferenceTests
    {
        ISolution _solution;
        MockFileSystem _fs;

        [SetUp]
        public void SetUp()
        {
            _solution = new FakeSolution(@"c:\test\fake.sln");
            _fs = new MockFileSystem();
        }

        IProject GetProject(string content, string projFileName = @"c:\test\one\fake1.csproj")
        {
            _fs.File.WriteAllText(projFileName, content);
            var project = new MockProject(_solution, _fs, new Logger(Verbosity.Quiet), projFileName);
            project.FileName = projFileName;
            project.Files.Add(new CSharpFile(project, @"c:\test\one\test.cs", "some c# code"));
            return project;
        }

        [Test]
        public void CanAddProjectReferenceWhenNoProjectReferencesExist()
        {
            var projectOne = GetProject(
                                 @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                    <ItemGroup>
                        <Compile Include=""Test.cs""/>
                    </ItemGroup>
                </Project>");
            projectOne.Title = "Project One";
            projectOne.ProjectId = Guid.NewGuid();
                
            var projectTwo = GetProject(
                                 @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                    <ItemGroup>
                        <Compile Include=""Test.cs""/>
                    </ItemGroup>
                </Project>", @"c:\test\two\fake2.csproj");

            _solution.Projects.Add(projectOne);
            _solution.Projects.Add(projectTwo);

            projectTwo.Files.Add(new CSharpFile(projectTwo, @"c:\test\two\test.cs", "some c# code"));
            var request = new AddReferenceRequest
            {
                Reference = @"fake1",
                FileName = @"c:\test\two\test.cs"
            };

            var handler = new AddReferenceHandler(_solution, new AddReferenceProcessorFactory(_solution, new IReferenceProcessor[] { new AddProjectReferenceProcessor(_solution) }, new NativeFileSystem()));
            handler.AddReference(request);

            var expectedXml = string.Format(@"
                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                    <ItemGroup>
                        <Compile Include=""Test.cs""/>
                    </ItemGroup>
                    <ItemGroup>
                        <ProjectReference Include=""..\one\fake1.csproj"">
                            <Project>{{{0}}}</Project>
                            <Name>Project One</Name>
                        </ProjectReference>
                    </ItemGroup>
                </Project>", projectOne.ProjectId.ToString().ToUpperInvariant());
            _fs.File.ReadAllText(projectTwo.FileName).ShouldEqualXml(expectedXml);
        }

        [Test]
        public void CanAddProjectReferenceWhenProjectReferencesExist()
        {
            var projectOne = GetProject(
                                 @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                                     <ItemGroup>
                                         <Compile Include=""Test.cs""/>
                                     </ItemGroup>
                                 </Project>");
            projectOne.Title = "Project One";
            projectOne.ProjectId = Guid.NewGuid();
            var project2Id = Guid.NewGuid();

            var projectTwo = GetProject(string.Format(@"
                 <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                     <ItemGroup>
                         <Compile Include=""Test.cs""/>
                     </ItemGroup>
                     <ItemGroup>
                         <ProjectReference Include=""..\existing\project.csproj"">
                             <Project>{{{0}}}</Project>
                             <Name>Existing Project</Name>
                         </ProjectReference>
                     </ItemGroup>
                 </Project>", project2Id), @"c:\test\two\fake2.csproj");

            projectTwo.Title = "Project Two";
            projectTwo.ProjectId = project2Id;
            projectTwo.Files.Add(new CSharpFile(projectTwo, @"c:\test\two\test.cs", "some c# code"));
            var expectedXml = string.Format(@"
                 <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                     <ItemGroup>
                         <Compile Include=""Test.cs""/>
                     </ItemGroup>
                     <ItemGroup>
                         <ProjectReference Include=""..\existing\project.csproj"">
                             <Project>{{{0}}}</Project>
                             <Name>Existing Project</Name>
                         </ProjectReference>
                         <ProjectReference Include=""..\one\fake1.csproj"">
                             <Project>{{{1}}}</Project>
                             <Name>Project One</Name>
                         </ProjectReference>
                     </ItemGroup>
                 </Project>", projectTwo.ProjectId,
                                  projectOne.ProjectId.ToString().ToUpperInvariant());

            _solution.Projects.Add(projectOne);
            _solution.Projects.Add(projectTwo);

            var request = new AddReferenceRequest
            {
                Reference = @"fake1",
                FileName = @"c:\test\two\test.cs"
            };

            var handler = new AddReferenceHandler(_solution, new AddReferenceProcessorFactory(_solution, new IReferenceProcessor[] { new AddProjectReferenceProcessor(_solution) }, new NativeFileSystem()));
            handler.AddReference(request);
            Console.Write(_fs.File.ReadAllText(projectTwo.FileName));
            _fs.File.ReadAllText(projectTwo.FileName).ShouldEqualXml(expectedXml);
        }

        [Test]
        public void WillNotAddDuplicateProjectReference()
        {
            var projectOne = GetProject(
                                 @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                                     <ItemGroup>
                                         <Compile Include=""Test.cs""/>
                                     </ItemGroup>
                                 </Project>");

            var projectTwoId = Guid.NewGuid();
            var xml = string.Format(@"
                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                    <ItemGroup>
                        <Compile Include=""Test.cs""/>
                    </ItemGroup>
                    <ItemGroup>
                        <ProjectReference Include=""..\one\fake1.csproj"">
                            <Project>{0}</Project>
                            <Name>Project One</Name>
                        </ProjectReference>
                    </ItemGroup>
                </Project>", string.Concat("{", projectOne.ProjectId.ToString().ToUpperInvariant(), "}"));
            var projectTwo = GetProject(xml, @"c:\test\two\fake2.csproj");
            projectTwo.Title = "Project Two";

            projectTwo.Files.Add(new CSharpFile(projectTwo, @"c:\test\two\test.cs", "some c# code"));

            projectTwo.ProjectId = projectTwoId;

            var expectedXml = XDocument.Parse(xml);

            _solution.Projects.Add(projectOne);
            _solution.Projects.Add(projectTwo);

            var request = new AddReferenceRequest
                {
                    Reference = @"fake1",
                    FileName = @"c:\test\two\test.cs"
                };

            var handler = new AddReferenceHandler(_solution, new AddReferenceProcessorFactory(_solution, new IReferenceProcessor[] { new AddProjectReferenceProcessor(_solution) }, new NativeFileSystem()));
            var response = handler.AddReference(request);

            projectTwo.AsXml().ToString().ShouldEqual(expectedXml.ToString());
            response.Message.ShouldEqual("Reference already added");
        }

        [Test]
        public void ShouldNotAddCircularReference()
        {
            var projectOne = GetProject(
                                 @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                                     <ItemGroup>
                                         <Compile Include=""Test.cs""/>
                                     </ItemGroup>
                                 </Project>");
            var xml = string.Format(@"
                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                    <ItemGroup>
                        <Compile Include=""Test.cs""/>
                    </ItemGroup>
                    <ItemGroup>
                        <ProjectReference Include=""..\one\fake1.csproj"">
                            <Project>{0}</Project>
                            <Name>Project One</Name>
                        </ProjectReference>
                    </ItemGroup>
                </Project>", string.Concat("{", projectOne.ProjectId.ToString().ToUpperInvariant(), "}"));
            var projectTwo = GetProject(xml, @"c:\test\two\fake2.csproj");
            projectTwo.Title = "Project Two";
            projectTwo.Files.Add(new CSharpFile(projectTwo, @"c:\test\two\test.cs", "some c# code"));
            projectTwo.AddReference(new ProjectReference(_solution, "Project One", projectOne.ProjectId));


            var expectedXml = XDocument.Parse(xml);
            
            _solution.Projects.Add(projectOne);
            _solution.Projects.Add(projectTwo);

            var request = new AddReferenceRequest
            {
                Reference = @"fake2",
                FileName = @"c:\test\one\test.cs"
            };

            var handler = new AddReferenceHandler(_solution, new AddReferenceProcessorFactory(_solution, new IReferenceProcessor[] { new AddProjectReferenceProcessor(_solution) }, new NativeFileSystem()));
            var response = handler.AddReference(request);

            projectTwo.AsXml().ToString().ShouldEqual(expectedXml.ToString());
            response.Message.ShouldEqual("Reference will create circular dependency");
        }
    }
}
