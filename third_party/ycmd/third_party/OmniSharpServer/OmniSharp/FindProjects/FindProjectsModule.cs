using Nancy;
namespace OmniSharp.FindProjects
{
    public class FindProjectsModule : NancyModule
    {
       public FindProjectsModule(FindProjectsHandler handler)
       {
           Post["Projects", "/projects"] = x =>
           {
               var res = handler.FindAllProjects();
               return Response.AsJson(res);
           };
       } 
    }
}
