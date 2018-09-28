using Nancy;
using Nancy.ModelBinding;

namespace OmniSharp.CurrentFileMembers
{
    public class CurrentFileMembersAsFlatModule : NancyModule
    {
        public CurrentFileMembersAsFlatModule
            (CurrentFileMembersHandler handler)
        {

            Post["CurrentFileMembersAsFlat", "/currentfilemembersasflat"] = x =>
            {
                var req = this.Bind<CurrentFileMembersRequest>();
                var members = handler.GetCurrentFileMembersAsFlat(req);
                return Response.AsJson(members);
            };
        }
    }
}
