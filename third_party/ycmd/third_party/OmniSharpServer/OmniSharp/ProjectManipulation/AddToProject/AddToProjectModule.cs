using Nancy;
using Nancy.ModelBinding;

namespace OmniSharp.ProjectManipulation.AddToProject
{
    public class AddToProjectModule : NancyModule
    {
        public AddToProjectModule(AddToProjectHandler handler)
        {
            Post["AddToProject", "/addtoproject"] = x =>
                {
                    var req = this.Bind<AddToProjectRequest>();
                    handler.AddToProject(req);
                    return Response.AsJson(true);
                };
        }
    }
}