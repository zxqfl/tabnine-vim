using Nancy;
using OmniSharp.Solution;
using Nancy.ModelBinding;

namespace OmniSharp.Build
{
    public class BuildCommandModule : NancyModule
    {
        public BuildCommandModule(BuildCommandBuilder commandBuilder)
        {
            Post["BuildCommand", "/buildcommand"] = x =>
                Response.AsText(commandBuilder.Executable.ApplyPathReplacementsForClient() + " " + commandBuilder.BuildArguments(true));

            Post["BuildTarget", "/buildtarget"] = x =>
            {
                var req = this.Bind<BuildTargetRequest>();
                return Response.AsJson(commandBuilder.BuildCommand(req));
            };
        }
    }
}
