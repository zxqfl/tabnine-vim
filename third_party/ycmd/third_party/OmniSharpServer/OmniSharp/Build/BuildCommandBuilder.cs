using System.IO;
using System.Linq;
using OmniSharp.Configuration;
using OmniSharp.Solution;

namespace OmniSharp.Build
{
    public class BuildCommandBuilder
    {
        private readonly ISolution _solution;
        private readonly OmniSharpConfiguration _config;
        private readonly Logger _logger;

        public BuildCommandBuilder(
            ISolution solution,
            OmniSharpConfiguration config,
            Logger logger)
        {
            _logger = logger;
            _solution = solution;
            _config = config;
        }

        public string Executable
        {
            get
            {
                return PlatformService.IsUnix
                    ? "xbuild"
                    : Path.Combine(
                    _config.MSBuildPath ?? System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(),
                    "Msbuild.exe");
            }
        }

        public string BuildArguments(bool includeSolutionFile)
        {
            
             var args = (PlatformService.IsUnix ? "" : "/m ") + "/nologo /v:q /property:GenerateFullPaths=true"; 
             if(includeSolutionFile)
             {
                 _logger.Debug("Adding solution " + _solution.FileName);
                 args += " \"" + _solution.FileName + "\""; 
             }
             return args;
        }

        public BuildTargetResponse BuildCommand(BuildTargetRequest req)
        {
            var command = Executable.ApplyPathReplacementsForClient() + " " + BuildArguments(false); 
            var file = _solution.FileName;
            var addConfiguration = true;

            if (!string.IsNullOrEmpty(req.Project))
            {
                var prj = _solution.Projects.FirstOrDefault(x => x.Title == req.Project);
                if (prj == null)
                {
                    _logger.Error("Could not find project with name " + req.Project + ". Falling back to building solution");
                }
                else
                {
                    _logger.Debug("Adding project " + prj.FileName + " to build command");
                    file = prj.FileName;
                    // Specifying a specific configuration will cause issues when dependent projects do not have the same configuration (Debug\Release) as the target project
                    addConfiguration = false;
                }
            }

            command += " \"" + file + "\" /target:" + req.Type;

            if(addConfiguration)
            {
                command += " /p:Configuration=" + req.Configuration;
            }



            var response = new BuildTargetResponse { Command = command };
            return response;
        }

    }
}
