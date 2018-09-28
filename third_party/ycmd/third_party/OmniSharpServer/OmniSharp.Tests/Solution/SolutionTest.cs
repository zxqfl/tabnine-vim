using System;
using System.IO;
using NUnit.Framework;
using OmniSharp;
using OmniSharp.Solution;
using Should;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;


namespace OmniSharp.Tests
{
    [TestFixture]
    public class FolderModeTest
    {
        [Test]
        public void Should_start_in_folder_mode()
        {
            var path = Environment.CurrentDirectory + "/Solution/minimal";
            var solution = new CSharpFolder(path, new Logger (Verbosity.Verbose), new FileSystem());
            solution.LoadSolution ();
        }

        [Test]
        public void Should_pick_unity_csharp_solution_in_folder_mode()
        {
            var fs = new MockFileSystem();
            fs.File.WriteAllText("Unity.sln", "");
            fs.File.WriteAllText("Unity-csharp.sln", "");
            var solution = new SolutionPicker(fs).LoadSolution(@".", new Logger(Verbosity.Verbose));
            fs.FileInfo.FromFileName (solution.FileName).Name.ShouldEqual("Unity-csharp.sln");
        }
    }

    [TestFixture]
    public class SolutionTest
    {
        readonly CSharpSolution _solution;

        public SolutionTest()
        {
            var path = Environment.CurrentDirectory + "/Solution/minimal/minimal.sln";
            _solution = new CSharpSolution (new FileSystem(), path, new Logger(Verbosity.Verbose));
            _solution.LoadSolution ();
        }

        [Test]
        public void Should_contain_one_orphan_project_and_one_real()
        {
            _solution.Projects.Count.ShouldEqual(2);
            _solution.Projects[1].Title.ShouldEqual("minimal");
        }

        [Test]
        public void Should_put_unknown_file_into_orphan_project()
        {
            _solution.ProjectContainingFile("test.cs").Title.ShouldEqual("Orphan Project");
        }

        [Test]
        public void Should_put_unknown_file_near_to_close_project_file()
        {
            _solution.ProjectContainingFile((Environment.CurrentDirectory + "/Solution/minimal/minimal/test.cs"))
                .Title.ShouldEqual("minimal");
        }

        [Test]
        public void Should_load_config_file()
        {
            var configLocation = Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "OmniSharp", "config.json");
            Configuration.ConfigurationLoader.Load(configLocation, null);
        }
    }
}

