using Nancy;
using OmniSharp.Solution;

namespace OmniSharp.ReloadSolution
{
    public class ReloadSolutionModule : NancyModule
    {
        public ReloadSolutionModule(ISolution solution)
        {
            Post["ReloadSolution", "/reloadsolution"] = x =>
                {
                    solution.Reload();
                    var config = Configuration.ConfigurationLoader.Config;
                    string mode = config.ClientPathMode.HasValue
                        ? config.ClientPathMode.Value.ToString()
                        : null;
                    Configuration.ConfigurationLoader.Load(config.ConfigFileLocation, mode);
                    return Response.AsJson(true);
                };
        }
    }
}
