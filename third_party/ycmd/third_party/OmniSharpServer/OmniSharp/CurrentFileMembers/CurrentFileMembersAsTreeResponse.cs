using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.TypeSystem;

namespace OmniSharp.CurrentFileMembers {

    public class CurrentFileMembersAsTreeResponse {
        public CurrentFileMembersAsTreeResponse() {}

        public CurrentFileMembersAsTreeResponse
            ( IEnumerable<IUnresolvedTypeDefinition> types
            , IDocument document) {

            this.TopLevelTypeDefinitions = types
                .Select(tld => Node.AsTree(tld, document));
        }

        /// <summary>
        ///   The types defined in a file. They contain their members
        ///   in a tree structure.
        /// </summary>
        public IEnumerable<Node> TopLevelTypeDefinitions {get; set;}
    }
}
