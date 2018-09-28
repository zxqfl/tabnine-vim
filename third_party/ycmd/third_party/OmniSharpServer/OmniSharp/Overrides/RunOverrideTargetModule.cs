using Nancy;
using Nancy.ModelBinding;

namespace OmniSharp.Overrides {
    public class RunOverrideTargetModule : NancyModule {

        public RunOverrideTargetModule
            (OverrideHandler overrideHandler) {

            Post["RunOverrideTarget", "/runoverridetarget"] = x =>
                {
                    var req = this.Bind<RunOverrideTargetRequest>();
                    var response = overrideHandler.RunOverrideTarget(req);
                    return Response.AsJson(response);
                };
        }
    }
}
