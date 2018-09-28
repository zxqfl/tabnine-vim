using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;
using Mono.Cecil;
using OmniSharp.Configuration;

namespace OmniSharp.Solution
{
    public class CSharpProject : Project
    {

        private readonly ISolution _solution;

        // public string FileName { get; private set; }

        public string AssemblyName { get; set; }

        // public Guid ProjectId { get; private set; }

        // public string Title { get; private set; }

        // public IProjectContent ProjectContent { get; set; }

        // public List<CSharpFile> Files { get; private set; }

        private readonly Logger _logger;

        IFileSystem _fileSystem;

        public CSharpProject(ISolution solution, 
                             Logger logger, 
                             string folderPath, 
                             IFileSystem fileSystem)
            : base(fileSystem, logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;
            _solution = solution;

            Files = new List<CSharpFile>();
            References = new List<IAssemblyReference>();

            DirectoryInfoBase folder;

            try
            {
                folder = _fileSystem.DirectoryInfo.FromDirectoryName(folderPath);
            }
            catch (DirectoryNotFoundException)
            {
                logger.Error("Directory not found - " + folderPath);
                return;
            }

            var files = folder.GetFiles("*.cs", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                _logger.Debug("Loading " + file.FullName);
                Files.Add(new CSharpFile(this, file.FullName));
            }

            this.ProjectContent = new CSharpProjectContent()
                .SetAssemblyName(AssemblyName)
                .AddAssemblyReferences(References)
                .AddOrUpdateFiles(Files.Select(f => f.ParsedFile));

            AddMsCorlib();
            AddReference(LoadAssembly(FindAssembly("System.Core")));


            var dlls = folder.GetFiles("*.dll", SearchOption.AllDirectories);
            foreach (var dll in dlls)
            {
                _logger.Debug("Loading assembly " + dll.FullName);
                AddReference(dll.FullName);
            }
        }

        public CSharpProject(ISolution solution,
                             IFileSystem fileSystem,
                             Logger logger, 
                             string title, 
                             string fileName, 
                             Guid id)
            : base(fileSystem, logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;
            _solution = solution;
            Title = title;
            if (fileSystem is FileSystem)
            {
                fileName = fileName.ForceNativePathSeparator();
            }
            FileName = fileName;
            ProjectId = id;
            Files = new List<CSharpFile>();
            Microsoft.Build.Evaluation.Project project;

            try
            {
                project = new Microsoft.Build.Evaluation.Project(_fileSystem, fileName);
            }
            catch (DirectoryNotFoundException)
            {
                logger.Error("Directory not found - " + FileName);
                return;
            }
            catch (FileNotFoundException)
            {
                logger.Error("File not found - " + FileName);
                return;
            }

            AssemblyName = project.GetPropertyValue("AssemblyName");

            SetCompilerSettings(project);

            AddCSharpFiles(project);

            References = new List<IAssemblyReference>();
            this.ProjectContent = new CSharpProjectContent()
                .SetAssemblyName(AssemblyName)
                .AddOrUpdateFiles(Files.Select(f => f.ParsedFile));

            AddMsCorlib();

            bool hasSystemCore = false;
            foreach (var item in project.GetItems("Reference"))
            {
                var assemblyFileName = GetAssemblyFileNameFromHintPath(project, item);
                //If there isn't a path hint or it doesn't exist, try searching
                if (assemblyFileName == null)
                    assemblyFileName = FindAssembly(item.EvaluatedInclude);

                //If it isn't in the search paths, try the GAC
                if (assemblyFileName == null && PlatformService.IsWindows)
                    assemblyFileName = FindAssemblyInNetGac(item.EvaluatedInclude);

                if (assemblyFileName != null)
                {
                    if (_fileSystem.Path.GetFileName(assemblyFileName).Equals("System.Core.dll", StringComparison.OrdinalIgnoreCase))
                        hasSystemCore = true;

                    _logger.Debug("Loading assembly " + item.EvaluatedInclude);
                    try
                    {
                        AddReference(LoadAssembly(assemblyFileName));
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e);
                    }

                }
                else
                    _logger.Error("Could not find referenced assembly " + item.EvaluatedInclude);
            }
            if (!hasSystemCore && FindAssembly("System.Core") != null)
                AddReference(LoadAssembly(FindAssembly("System.Core")));


            AddProjectReferences(project);
        }

        string GetAssemblyFileNameFromHintPath(Microsoft.Build.Evaluation.Project p, Microsoft.Build.Evaluation.ProjectItem item)
        {
            string assemblyFileName = null;
            if (item.HasMetadata("HintPath"))
            {
                assemblyFileName = _fileSystem.Path.Combine(p.DirectoryPath, item.GetMetadataValue("HintPath")).ForceNativePathSeparator();
                _logger.Info("Looking for assembly from HintPath at " + assemblyFileName);
                if (!_fileSystem.File.Exists(assemblyFileName))
                {
                    _logger.Info("Did not find assembly from HintPath");
                    assemblyFileName = null;
                }
            }
            return assemblyFileName;
        }

        void SetCompilerSettings(Microsoft.Build.Evaluation.Project p)
        {
            CompilerSettings = new CompilerSettings
            {
                AllowUnsafeBlocks = GetBoolProperty(p, "AllowUnsafeBlocks") ?? false,
                CheckForOverflow = GetBoolProperty(p, "CheckForOverflowUnderflow") ?? false
            };
            string[] defines = p.GetPropertyValue("DefineConstants")
                .Split(new [] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string define in defines)
                CompilerSettings.ConditionalSymbols.Add(define);

            var config = ConfigurationLoader.Config;
            foreach (var define in config.Defines)
                CompilerSettings.ConditionalSymbols.Add(define);
        }

        void AddMsCorlib()
        {
            string mscorlib = FindAssembly("mscorlib");
            if (mscorlib != null)
                AddReference(LoadAssembly(mscorlib));
            else
                _logger.Error("Could not find mscorlib");
        }

        void AddCSharpFiles(Microsoft.Build.Evaluation.Project p)
        {
            foreach (var item in p.GetItems("Compile"))
            {
                try
                {
                    string path = _fileSystem.Path.Combine(p.DirectoryPath, item.EvaluatedInclude).ForceNativePathSeparator();

                    if (_fileSystem.File.Exists(path))
                    {
                        string file = _fileSystem.FileInfo.FromFileName(path).FullName;
                        _logger.Debug("Loading " + file);
                        Files.Add(new CSharpFile(this, file));
                    }
                    else
                    {
                        _logger.Error("File does not exist - " + path);
                    }
                }
                catch (NullReferenceException e)
                {
                    _logger.Error(e);
                }
            }
        }

        void AddProjectReferences(Microsoft.Build.Evaluation.Project p)
        {
            foreach (Microsoft.Build.Evaluation.ProjectItem item in p.GetItems("ProjectReference"))
            {
                var projectName = item.HasMetadata("Name") 
                    ? item.GetMetadataValue("Name") 
                    : Path.GetFileNameWithoutExtension(item.EvaluatedInclude);

                var referenceGuid = Guid.Parse(item.GetMetadataValue("Project"));
                _logger.Debug("Adding project reference {0}, {1}", projectName, referenceGuid);
                AddReference(new ProjectReference(_solution, projectName, referenceGuid));
            }
        }

        public override string ToString()
        {
            return string.Format("[CSharpProject AssemblyName={0}]", AssemblyName);
        }

        string FindAssemblyInNetGac(string evaluatedInclude)
        {
            try
            {
                AssemblyNameReference assemblyNameReference = AssemblyNameReference.Parse(evaluatedInclude);
                return GacInterop.FindAssemblyInNetGac(assemblyNameReference);
            }
            catch (TypeInitializationException)
            {
                _logger.Debug("Fusion not available - cannot get {0} from the gac.", evaluatedInclude);
                return null;
            }
        }

        static bool? GetBoolProperty(Microsoft.Build.Evaluation.Project p, string propertyName)
        {
            string val = p.GetPropertyValue(propertyName);
            if (val.Equals("true", StringComparison.OrdinalIgnoreCase))
                return true;
            if (val.Equals("false", StringComparison.OrdinalIgnoreCase))
                return false;
            return null;
        }
    }
}
