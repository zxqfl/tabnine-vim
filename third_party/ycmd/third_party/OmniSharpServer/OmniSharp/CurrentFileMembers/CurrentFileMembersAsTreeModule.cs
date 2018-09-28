using Nancy;
using Nancy.ModelBinding;

namespace OmniSharp.CurrentFileMembers
{
    public class CurrentFileMembersAsTreeModule : NancyModule
    {
        public CurrentFileMembersAsTreeModule
            (CurrentFileMembersHandler handler)
        {

            Post["CurrentFileMembersAsTree", "/currentfilemembersastree"] = x =>
            {
                var req = this.Bind<CurrentFileMembersRequest>();
                var members = handler.GetCurrentFileMembersAsTree(req);
                return Response.AsJson(members);
            };
        }
    }
}
