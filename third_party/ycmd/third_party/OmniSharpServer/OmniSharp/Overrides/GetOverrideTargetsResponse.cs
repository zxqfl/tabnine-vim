using System;
using ICSharpCode.NRefactory.CSharp.Refactoring;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.CSharp.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem;

namespace OmniSharp.Overrides {

    public class GetOverrideTargetsResponse {

        public GetOverrideTargetsResponse() {}

        public GetOverrideTargetsResponse
            ( IMember m
            , CSharpTypeResolveContext resolveContext) {
            if (resolveContext == null)
                throw new ArgumentNullException("resolveContext");

            if (m == null)
                throw new ArgumentNullException("m");

            this.OverrideTargetName =
                GetOverrideTargetName(m, resolveContext);
        }

        /// <summary>
        ///   A human readable signature of the member that is to be
        ///   overridden.
        /// </summary>
        public string OverrideTargetName {get; set;}

        public static string GetOverrideTargetName
            (IMember m, CSharpTypeResolveContext resolveContext) {
            var builder = new TypeSystemAstBuilder
                (new CSharpResolver(resolveContext));

			return builder.ConvertEntity(m).ToString()
                // Builder automatically adds a trailing newline
                .TrimEnd(Environment.NewLine.ToCharArray());
        }


    }

}
