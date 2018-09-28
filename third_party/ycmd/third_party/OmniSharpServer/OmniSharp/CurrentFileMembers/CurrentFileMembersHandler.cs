using System.Collections.Generic;
using System.Linq;
using OmniSharp.AutoComplete;
using OmniSharp.Common;
using OmniSharp.Parser;
using System.Diagnostics;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;

namespace OmniSharp.CurrentFileMembers
{

    public class CurrentFileMembersHandler
    {
        private readonly BufferParser _parser;

        public CurrentFileMembersHandler(BufferParser parser)
        {
            _parser = parser;
        }

        /// <summary>
        ///   Returns a representation of the current buffer's members
        ///   and their locations. The caller may build a UI that lets
        ///   the user navigate to them quickly.
        /// </summary>
        public CurrentFileMembersAsTreeResponse GetCurrentFileMembersAsTree(CurrentFileMembersRequest request)
        {
            var context = new BufferContext(request, this._parser);

            var typesDefinedInThisFile = context.ParsedContent
                .UnresolvedFile.TopLevelTypeDefinitions;

            return new CurrentFileMembersAsTreeResponse(typesDefinedInThisFile, context.Document);
        }

        /// <summary>
        ///   Like GetCurrentFileMembersAsTree() but the result has no
        ///   tree hierarchy and is completely flat. The Locations
        ///   appear in the order they are in the given file.
        /// </summary>
        public IEnumerable<QuickFix> GetCurrentFileMembersAsFlat(CurrentFileMembersRequest request)
        {
            if (!request.ShowAccessModifiers)
            {
                return GetCurrentFileMembersAsFlatWithoutAccessModifiers(request);
            }

            // Get and flatten a tree response.
            var treeResponse = this.GetCurrentFileMembersAsTree(request); 

            // Ensure all topLevelTypeDefinitions have their members
            // right after them in the response
            var locationsOfTypesAndChildren = treeResponse
                .TopLevelTypeDefinitions
                        .SelectMany(tld => new[] { tld.Location }
                            .Concat(tld.ChildNodes
                                    .Select(cn => cn.Location)));

            return locationsOfTypesAndChildren;
        }

        public IEnumerable<QuickFix> GetCurrentFileMembersAsFlatWithoutAccessModifiers(CurrentFileMembersRequest request)
        {
            var context = new BufferContext(request, this._parser);

            var result = new List<QuickFix>();
           

            foreach (var item in context.ParsedContent.UnresolvedFile.TopLevelTypeDefinitions)
            {
                result.Add(new QuickFix()
                    {
                        FileName = item.Region.FileName
                            , Line = item.Region.BeginLine
                            , Column = item.Region.BeginColumn
                            , EndLine = item.Region.EndLine
                            , EndColumn = item.Region.EndColumn
                            , Text = item.Name
                    });
            }

            var members = context.ParsedContent.UnresolvedFile.TopLevelTypeDefinitions
                .SelectMany(x => x.Members);



            foreach (var item in members)
            {
                var ambience = new CSharpAmbience();
                ambience.ConversionFlags = ConversionFlags.ShowParameterList | ConversionFlags.ShowParameterNames;
                var memberTitle = ambience.ConvertSymbol(item.Resolve(context.ResolveContext));

                var qf = new QuickFix()
                {
                    FileName = item.Region.FileName
                            , Line = item.Region.BeginLine
                            , Column = item.Region.BeginColumn
                            , EndLine = item.Region.EndLine
                            , EndColumn = item.Region.EndColumn
                            , Text = memberTitle
                };

                result.Add(qf);
            }

            return result.OrderBy(x => x.Text);

        }
    }
}
