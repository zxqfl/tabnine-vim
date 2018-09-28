using System.Collections.Generic;
using System.Linq;
using Nancy;
using Nancy.ModelBinding;
using OmniSharp.Common;

namespace OmniSharp.CurrentFileMembers {

    /// <summary>
    ///   Returns the locations of only the TopLevelTypeDefinitions in
    ///   the given file.
    /// </summary>
    public class CurrentFileTopLevelTypesModule : NancyModule {
        public CurrentFileTopLevelTypesModule(CurrentFileMembersHandler handler) {
            Post["CurrentFileTopLevelTypes", "/currentfiletopleveltypes"] = x => {
                var req = this.Bind<CurrentFileMembersRequest>();
                var members = handler.GetCurrentFileMembersAsTree(req);

                IEnumerable<QuickFix> topLevelTypeDefinitions =
                    members.TopLevelTypeDefinitions
                    .Select(m => m.Location);

                return Response.AsJson(topLevelTypeDefinitions);
            };
        }
    }
}
