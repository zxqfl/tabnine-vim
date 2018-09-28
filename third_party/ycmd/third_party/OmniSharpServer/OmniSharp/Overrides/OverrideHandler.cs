using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Refactoring;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Editor;
using OmniSharp.Common;
using OmniSharp.Configuration;
using OmniSharp.Parser;
using OmniSharp.Refactoring;

namespace OmniSharp.Overrides {
    public class OverrideHandler {

        private readonly BufferParser _parser;
        private readonly OmniSharpConfiguration _config;

        public OverrideHandler(BufferParser parser, OmniSharpConfiguration config) {
            _parser = parser;
            _config = config;
        }

        /// <summary>
        ///   Returns the available overridable members in the given
        ///   request.
        /// </summary>
        public IEnumerable<GetOverrideTargetsResponse> GetOverrideTargets
            (Request request) {
            var overrideContext = new OverrideContext(request, _parser);

            return overrideContext.OverrideTargets;
        }

        /// <summary>
        ///   Takes an editing context. Inserts an override
        ///   declaration of the chosen member in the context. Returns
        ///   the new context.
        /// </summary>
        /// <remarks>
        ///   The text editor cursor stays in the same line and column
        ///   it originally was.
        /// </remarks>
        public RunOverrideTargetResponse RunOverrideTarget
            (RunOverrideTargetRequest request) {
            var overrideContext = new OverrideContext(request, _parser);
            var refactoringContext = OmniSharpRefactoringContext.GetContext
                (overrideContext.BufferParser, request);

            var memberToOverride = overrideContext.GetOverridableMembers()
                .First(ot => {
                    var memberSignature =
                     GetOverrideTargetsResponse.GetOverrideTargetName
                      (ot, overrideContext.CompletionContext.ResolveContext);

                    return memberSignature == request.OverrideTargetName;});

            var builder = new TypeSystemAstBuilder
                (new CSharpResolver
                 (overrideContext.CompletionContext.ResolveContext))
                 // Will generate a "throw new
                 // NotImplementedException();" statement in the
                 // bodies
                 {GenerateBody = true};

            var newEditorContents = runOverrideTargetWorker
                ( request
                , refactoringContext
                , overrideContext.CompletionContext.ParsedContent
                , script: new OmniSharpScript(refactoringContext, _config)
                , memberDeclaration:
                    builder.ConvertEntity(memberToOverride));

            return new RunOverrideTargetResponse
                ( fileName : request.FileName
                , buffer   : newEditorContents.Text
                , line     : request.Line
                , column   : request.Column);
        }

        /// <summary>
        ///   Inserts the given EntityDeclaration with the given
        ///   script at the end of the type declaration under the
        ///   cursor (e.g. class / struct).
        /// </summary>
        /// <remarks>
        ///   Alters the given script. Returns its CurrentDocument
        ///   property. Alters the given memberDeclaration, adding
        ///   Modifiers.Override to its Modifiers as well as removing
        ///   Modifiers.Virtual.
        /// </remarks>
        IDocument runOverrideTargetWorker
            ( Request                     request
            , OmniSharpRefactoringContext refactoringContext
            , ParsedResult                parsedContent
            , EntityDeclaration           memberDeclaration
            , OmniSharpScript             script) {

            // Add override flag
            memberDeclaration.Modifiers |= Modifiers.Override;
            // Remove virtual flag
            memberDeclaration.Modifiers &= ~ Modifiers.Virtual;
            // Remove abstract flag
            memberDeclaration.Modifiers &= ~ Modifiers.Abstract;

            // The current type declaration, e.g. class, struct..
            var typeDeclaration = parsedContent.SyntaxTree.GetNodeAt
                ( refactoringContext.Location
                , n => n.NodeType == NodeType.TypeDeclaration);

            // Even empty classes have nodes, so this works
            var lastNode =
                typeDeclaration.Children.Last();

            script.InsertBefore
                ( node    : lastNode
                , newNode : memberDeclaration);
            script.FormatText(memberDeclaration);

            return script.CurrentDocument;
        }

    }
}
