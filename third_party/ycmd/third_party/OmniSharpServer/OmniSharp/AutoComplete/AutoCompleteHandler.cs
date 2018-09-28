using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Completion;
using ICSharpCode.NRefactory.Completion;
using OmniSharp.Parser;
using OmniSharp.Solution;
using OmniSharp.Configuration;

namespace OmniSharp.AutoComplete
{
    public class AutoCompleteHandler
    {
        private readonly ISolution _solution;
        private readonly BufferParser _parser;
        private readonly Logger _logger;
        private readonly  OmniSharpConfiguration _config;

        public AutoCompleteHandler(ISolution solution, BufferParser parser, Logger logger, OmniSharpConfiguration config)
        {
            _solution = solution;
            _parser = parser;
            _logger = logger;
            _config = config;
        }

        public IEnumerable<CompletionData> CreateProvider(AutoCompleteRequest request)
        {
            request.Column = request.Column - request.WordToComplete.Length;
            var completionContext = new BufferContext(request, _parser);

            var partialWord = request.WordToComplete;

            var project = _solution.ProjectContainingFile(request.FileName);
        
            var contextProvider = new DefaultCompletionContextProvider(completionContext.Document, completionContext.ParsedContent.UnresolvedFile);

            if (project.CompilerSettings != null)
            {
                var conditionalSymbols = project.CompilerSettings.ConditionalSymbols;
                foreach (var symbol in conditionalSymbols)
                {
                    contextProvider.AddSymbol(symbol);
                }
            }

            var instantiating = IsInstantiating(completionContext.NodeCurrentlyUnderCursor);

            var engine = new CSharpCompletionEngine(completionContext.Document
                , contextProvider
                , new CompletionDataFactory(project
                    , partialWord
                    , instantiating
                    , request
                    , _config)
                , completionContext.ParsedContent.ProjectContent
                , completionContext.ResolveContext)
            {
                EolMarker = Environment.NewLine
            };
            engine.AutomaticallyAddImports = request.WantImportableTypes;
            _logger.Debug("Getting Completion Data");

            IEnumerable<CompletionData> data = engine.GetCompletionData(completionContext.CursorPosition, request.ForceSemanticCompletion.GetValueOrDefault(true)).Cast<CompletionData>();

            _logger.Debug("Got Completion Data");
            return data.Where(d => d != null && d.CompletionText.IsValidCompletionFor(partialWord))
                       .FlattenOverloads()
                       .RemoveDupes()
                       .OrderByDescending(d => d.RequiredNamespaceImport != null ? 0 : 1)
                       .ThenByDescending(d => d.CompletionText.IsValidCompletionStartsWithExactCase(partialWord))
                       .ThenByDescending(d => d.CompletionText.IsValidCompletionStartsWithIgnoreCase(partialWord))
                       .ThenByDescending(d => d.CompletionText.IsCamelCaseMatch(partialWord))
                       .ThenByDescending(d => d.CompletionText.IsSubsequenceMatch(partialWord))
                       .ThenBy(d => d.CompletionText);
        }

        private static bool IsInstantiating(AstNode nodeUnderCursor)
        {
            bool instantiating = false;

            if (nodeUnderCursor != null
                && nodeUnderCursor.Parent != null
                && nodeUnderCursor.Parent.Parent != null)
            {
                instantiating =
                    nodeUnderCursor.Parent.Parent.Children.Any(child => child.Role.ToString() == "new");
            }
            return instantiating;
        }
    }
}
