using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;
using OmniSharp.AutoComplete;
using OmniSharp.Common;
using OmniSharp.Parser;

namespace OmniSharp.Overrides {

    /// <summary>
    ///   Represents a context where some base class members are going
    ///   to be overridden.
    /// </summary>
    public class OverrideContext {

        readonly IUnresolvedTypeDefinition currentTypeDefinition;

        public OverrideContext
            (Request request, BufferParser parser) {

            this.BufferParser = parser;
            this.CompletionContext = new BufferContext
                (request, this.BufferParser);

            currentTypeDefinition = this.CompletionContext.ParsedContent
                .UnresolvedFile.GetInnermostTypeDefinition
                (this.CompletionContext.TextLocation);
            this.CurrentType = currentTypeDefinition.Resolve(this.CompletionContext.ResolveContext);

            this.OverrideTargets =
                GetOverridableMembers()
                .Select(m => new GetOverrideTargetsResponse
                        (m, this.CompletionContext.ResolveContext))
                .ToArray();
        }

        /// <summary>
        ///   The type currently under the cursor in this context.
        /// </summary>
        public IType CurrentType {get; private set;}
        public IEnumerable<GetOverrideTargetsResponse> OverrideTargets {get; private set;}
        public BufferContext CompletionContext {get; private set;}

        public BufferParser BufferParser {get; private set;}

        public IEnumerable<IMember> GetOverridableMembers() {
            // Disallow trying to override in e.g. interfaces or enums
            if (this.CurrentType.Kind != TypeKind.Class
                && this.CurrentType.Kind != TypeKind.Struct)
                return new IMember[0];

            var candidates = this.CurrentType
                .GetMembers(m => (m.IsVirtual || m.IsAbstract) && m.IsOverridable).ToArray();

            var overridden = this.CurrentType
                .GetMembers(m => m.IsOverride && 
                    m.DeclaringTypeDefinition == currentTypeDefinition)
                .SelectMany(m => InheritanceHelper.GetBaseMembers(m, true));

            return candidates.Except (overridden);
        }
    }
}
