using Nancy;
using Nancy.ModelBinding;

namespace OmniSharp.GotoRegion {
    public class GotoRegionModule : NancyModule {
        public GotoRegionModule
            (GotoRegionHandler handler) {

            Post["GotoRegion", "/gotoregion"] = x =>
            {
                var req = this.Bind<Common.Request>();
                var members = handler.GetFileRegions(req);
                return Response.AsJson(members);
            };
        }
    }
}
