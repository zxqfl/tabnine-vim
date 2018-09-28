using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Refactoring;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Semantics;
using OmniSharp.Common;
using OmniSharp.Configuration;
using OmniSharp.Parser;
using OmniSharp.Refactoring;

namespace OmniSharp.CodeIssues
{
    public class FixUsingsHandler
    {
        readonly BufferParser _bufferParser;
        readonly OmniSharpConfiguration _config;
        string _fileName;
        Logger _logger;

        public FixUsingsHandler(BufferParser bufferParser, Logger logger, OmniSharpConfiguration config)
        {
            _bufferParser = bufferParser;
            _logger = logger;
            _config = config;
        }


        public FixUsingsResponse FixUsings(Request request)
        {
            _fileName = request.FileName;
            string buffer = RemoveUsings(request.Buffer);
            buffer = SortUsings(buffer);
            buffer = AddLinqForQueryIfMissing(buffer);

            bool ambiguousResultsFound = false;
            bool usingsAdded = true;

            while (usingsAdded)
            {
                var content = _bufferParser.ParsedContent(buffer, _fileName);
                var tree = content.SyntaxTree;

                var resolver = new CSharpAstResolver(content.Compilation, content.SyntaxTree, content.UnresolvedFile);
                var unresolvedNodes = GetAllUnresolvedNodes(tree, resolver).Select(nr => GetNodeToAddUsing(nr));
                usingsAdded = false;
                request.Buffer = buffer;
                var outerContext = OmniSharpRefactoringContext.GetContext(_bufferParser, request);
                using (var script = new OmniSharpScript(outerContext, _config))
                {
                    foreach (var unresolvedNode in unresolvedNodes)
                    {
                        _logger.Info(unresolvedNode);

                        var requestForNode = CreateRequest(buffer, unresolvedNode);
                        var innerContext = OmniSharpRefactoringContext.GetContext(_bufferParser, requestForNode);
                        var addUsingAction = new AddUsingAction();
                        var actions = addUsingAction.GetActions(innerContext).Where(a => a.Description.StartsWith("using")).ToArray();

                        if (actions.Length == 1)
                        {
                            var a = actions[0];
                            _logger.Info("Adding " + a.Description);
                            a.Run(script);
                            usingsAdded = true;
                            break;
                        }
                        ambiguousResultsFound |= actions.Length > 1;
                    }
                }
                buffer = outerContext.Document.Text;
            }

            IEnumerable<QuickFix> ambiguous = Enumerable.Empty<QuickFix>();
            if (ambiguousResultsFound)
            {
                ambiguous = GetAmbiguousNodes(buffer, request.FileName);
            }
            return new FixUsingsResponse(buffer, ambiguous);
        }

        IEnumerable<QuickFix> GetAmbiguousNodes(string buffer, string fileName)
        {
            var ambiguous = new List<QuickFix>();
            var content = _bufferParser.ParsedContent(buffer, fileName);
            var resolver = new CSharpAstResolver(content.Compilation, content.SyntaxTree, content.UnresolvedFile);


            IEnumerable<NodeResolved> nodes = GetResolvedNodes(content.SyntaxTree, resolver)
                    .Where(n => n.ResolveResult is UnknownMemberResolveResult
                                                  || n.ResolveResult is UnknownIdentifierResolveResult);

            foreach (var unidentifiedNode in nodes.Select(r => GetNodeToAddUsing(r)).Distinct().OrderBy(n => n.StartLocation))
            {
                ambiguous.Add(new QuickFix
                    { 
                        Column = unidentifiedNode.StartLocation.Column,
                        Line = unidentifiedNode.StartLocation.Line,
                        FileName = fileName,
                        Text = "`" + unidentifiedNode + "`" + " is ambiguous"
                    });
            }
            return ambiguous;
        }

        static AstNode GetNodeToAddUsing(NodeResolved node)
        {
            string name = null;
            AstNode astNode = node.Node;
            var unknownIdentifierResolveResult = node.ResolveResult as UnknownIdentifierResolveResult;
            if (unknownIdentifierResolveResult != null)
            {
                name = unknownIdentifierResolveResult.Identifier;
            }

            if (node.ResolveResult is UnknownMemberResolveResult)
            {
                name = (node.ResolveResult as UnknownMemberResolveResult).MemberName;
            }
            return astNode.Descendants.FirstOrDefault(n => n.ToString() == name);
        }

        string RunActions(OmniSharpRefactoringContext context, IEnumerable<CodeAction> actions)
        {
            using (var script = new OmniSharpScript(context, _config))
            {
                foreach (var action in actions)
                {
                    if (action != null)
                    {
                        action.Run(script);
                    }
                }
            }
            return context.Document.Text;
        }

        string RemoveUsings(string buffer)
        {
            var content = _bufferParser.ParsedContent(buffer, _fileName);
            var tree = content.SyntaxTree;
            var firstUsing = tree.Children.FirstOrDefault(IsUsingDeclaration);

            if (firstUsing != null)
            {
                var request = CreateRequest(buffer, firstUsing);
                var context = OmniSharpRefactoringContext.GetContext(_bufferParser, request);
                var redundantUsings = new RedundantUsingDirectiveIssue().GetIssues(context, null);
                if (redundantUsings.Any())
                {
                    var actions = redundantUsings.First().Actions;
                    buffer = RunActions(context, actions);
                }
            }

            return buffer;
        }

        Request CreateRequest(string buffer, AstNode node)
        {
            var request = new Request();
            request.Buffer = buffer;
            request.Line = node.Region.BeginLine;
            request.Column = node.Region.BeginColumn;
            request.FileName = _fileName;
            return request;
        }

        string SortUsings(string buffer)
        {
            var content = _bufferParser.ParsedContent(buffer, _fileName);
            var tree = content.SyntaxTree;
            var firstUsing = tree.Descendants.FirstOrDefault(IsUsingDeclaration);

            if (firstUsing != null)
            {
                var request = CreateRequest(buffer, firstUsing);
                var context = OmniSharpRefactoringContext.GetContext(_bufferParser, request);
                var actions = new SortUsingsAction().GetActions(context);
                buffer = RunActions(context, actions);

            }

            return buffer;
        }

        static NodeResolved GetNextUnresolvedNode(AstNode tree, CSharpAstResolver resolver)
        {
            return GetNextUnresolvedNode(tree, tree.FirstChild, resolver);
        }

        static NodeResolved GetNextUnresolvedNode(AstNode tree, AstNode after, CSharpAstResolver resolver)
        {
            var nodes = GetResolvedNodes(tree, after, resolver);
            var node = nodes.FirstOrDefault(n => n.ResolveResult is UnknownIdentifierResolveResult);
            if (node == null)
            {
                node = nodes.FirstOrDefault(n => n.ResolveResult is UnknownMemberResolveResult);
            }
            return node;
        }

        static IEnumerable<NodeResolved> GetResolvedNodes(AstNode tree, CSharpAstResolver resolver)
        {
            return GetResolvedNodes(tree, tree.FirstChild, resolver);
        }

        static IEnumerable<NodeResolved> GetAllUnresolvedNodes(AstNode tree, CSharpAstResolver resolver)
        {
            var nodes = tree.Descendants.OrderBy(n => n.StartLocation).Select(n => new NodeResolved
                {
                    Node = n,
                    ResolveResult = resolver.Resolve(n)
                });

            return nodes.Where(n => n.ResolveResult is UnknownIdentifierResolveResult ||
                n.ResolveResult is UnknownMemberResolveResult);
        }

        static IEnumerable<NodeResolved> GetResolvedNodes(AstNode tree, AstNode after, CSharpAstResolver resolver)
        {
            return tree.Descendants.Distinct().SkipWhile(n => n != after).Skip(1).OrderBy(n => n.StartLocation).Select(n => new NodeResolved
                {
                    Node = n,
                    ResolveResult = resolver.Resolve(n)
                });
        }

        string AddLinqForQueryIfMissing(string buffer)
        {
            var content = _bufferParser.ParsedContent(buffer, _fileName);
            var tree = content.SyntaxTree;
            if (!tree.Descendants.OfType<UsingDeclaration>().Any(u => u.Namespace.Equals("System.Linq")))
            {
                var linqQuery = tree.Descendants.FirstOrDefault(n => n.NodeType == NodeType.QueryClause);
                if (linqQuery != null)
                {
                    buffer = AddUsingLinq(linqQuery, buffer);
                }
            }
            return buffer;
        }

        string AddUsingLinq(AstNode astNode, string buffer)
        {
            var request = CreateRequest(buffer, astNode);
            var context = OmniSharpRefactoringContext.GetContext(_bufferParser, request);
            var script = new OmniSharpScript(context, _config);
            UsingHelper.InsertUsingAndRemoveRedundantNamespaceUsage(context, script, "System.Linq");
            return context.Document.Text;
        }

        static bool IsUsingDeclaration(AstNode node)
        {
            return node is UsingDeclaration || node is UsingAliasDeclaration;
        }

        class NodeResolved
        {
            public AstNode Node { get; set; }
            public ResolveResult ResolveResult { get; set; }
        }
    }
}
