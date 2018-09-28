using OmniSharp.Common;

namespace OmniSharp.Build
{
    public class BuildTargetRequest : Request
    {
        public string Project { get; set; }

        public BuildType Type { get; set; }

        public BuildConfiguration Configuration { get; set; }

        public enum BuildType
        {
            Build,
            Rebuild,
            Clean
        }

        public enum BuildConfiguration
        {
            Debug,
            Release
        }
    }

}
