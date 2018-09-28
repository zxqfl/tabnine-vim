using System;
using OmniSharp.Solution;
using System.IO.Abstractions;

namespace OmniSharp
{
    public class SolutionPicker
    {
        readonly IFileSystem _fileSystem;

        public SolutionPicker(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public ISolution LoadSolution(string solutionPath, Logger logger)
        {
            solutionPath = solutionPath.ApplyPathReplacementsForServer ();
            if (_fileSystem.Directory.Exists (solutionPath))
            {
                var unitySolutions = GetUnitySolutions(solutionPath);
                if (unitySolutions.Length == 1)
                {
                    solutionPath = unitySolutions[0];
                    logger.Debug ("Found solution file - " + solutionPath);
                    return new CSharpSolution (_fileSystem, solutionPath, logger);
                }

                var slnFiles = _fileSystem.Directory.GetFiles (solutionPath, "*.sln");
                if (slnFiles.Length == 1)
                {
                    solutionPath = slnFiles [0];
                    logger.Debug ("Found solution file - " + solutionPath);
                    return new CSharpSolution (_fileSystem, solutionPath, logger);
                }
                return new CSharpFolder (solutionPath, logger, _fileSystem);
            }
            return new CSharpSolution (_fileSystem, solutionPath, logger);
        }

        string[] GetUnitySolutions(string solutionPath)
        {
            return _fileSystem.Directory.GetFiles(solutionPath, "*-csharp.sln");
        }
    }
}

