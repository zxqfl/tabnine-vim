using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.TypeSystem;
using OmniSharp.Common;
using OmniSharp.Parser;

namespace OmniSharp.CurrentFileMembers {

    /// <summary>
    ///   Represents a node in an abstract syntax tree.
    /// </summary>
    public class Node {
        public Node() {}

        /// <summary>
        ///   Initializes a new instance with no child nodes for the
        ///   given member.
        /// </summary>
        public Node(IUnresolvedMember member, IDocument document) {
            this.ChildNodes = null;
            this.Location = QuickFix.ForNonBodyRegion(member, document);

            // Fields' BodyRegion does not include their name for some
            // reason. To prevent the field's name missing, include
            // the whole region for them.
			if (member.SymbolKind == SymbolKind.Field)
                this.Location.Text += member.Name;
        }

        public IEnumerable<Node> ChildNodes {get; set;}
        public QuickFix          Location   {get; set;}

        public static Node AsTree
            ( IUnresolvedTypeDefinition topLevelTypeDefinition
            , IDocument document) {

            var retval = new Node()
                { ChildNodes = topLevelTypeDefinition.Members
                    .Select(m => new Node(m, document))
                , Location = QuickFix.ForNonBodyRegion
                    (topLevelTypeDefinition, document)};

            return retval;
        }

    }
}
