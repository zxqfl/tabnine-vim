using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.CSharp.TypeSystem;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using MonoDevelop.Ide.FindInFiles;
using OmniSharp.Common;
using OmniSharp.Extensions;
using OmniSharp.Parser;
using OmniSharp.Solution;

namespace OmniSharp.FindUsages
{
    public class FindUsagesHandler
    {
        private readonly BufferParser _parser;
        private readonly ISolution _solution;
        private ConcurrentBag<AstNode> _result;
        private ProjectFinder _projectFinder;

        public FindUsagesHandler(BufferParser parser, ISolution solution, ProjectFinder projectFinder)
        {
            _projectFinder = projectFinder;
            _parser = parser;
            _solution = solution;
        }

        public QuickFixResponse FindUsages(FindUsagesRequest request)
        {
            var result = FindUsageNodes(request)
                            .Distinct(new NodeComparer())
                            .OrderBy(n => n.GetRegion().FileName)
                            .ThenBy(n => n.StartLocation.Line)
                            .ThenBy(n => n.StartLocation.Column);
                            
            var res = new QuickFixResponse();
            if (result.Any())
            {
                var usages = result.Select(node => new QuickFix
                {
                    FileName = node.GetRegion().FileName,
                    Text = node.Preview(_solution.GetFile(node.GetRegion().FileName), request.MaxWidth).Replace("'", "''"),
                    Line = node.StartLocation.Line,
                    Column = node.StartLocation.Column,
                });
                res.QuickFixes = usages;
            }

            return res;
        }

        public IEnumerable<AstNode> FindUsageNodes(Request request)
        {
            var res = _parser.ParsedContent(request.Buffer, request.FileName);
            var loc = new TextLocation(request.Line, request.Column);
            _result = new ConcurrentBag<AstNode>();
            var findReferences = new FindReferences
            {
                FindCallsThroughInterface = true,
                FindCallsThroughVirtualBaseMethod = true,
                FindTypeReferencesEvenIfAliased = false,
            };

            ResolveResult resolveResult = ResolveAtLocation.Resolve(res.Compilation, res.UnresolvedFile, res.SyntaxTree, loc);
            if (resolveResult is LocalResolveResult)
            {
                var variable = (resolveResult as LocalResolveResult).Variable;
                findReferences.FindLocalReferences(variable, res.UnresolvedFile, res.SyntaxTree, res.Compilation,
                    (node, rr) => _result.Add(node.GetDefinition()), CancellationToken.None);
            }
            else
            {
                IEntity entity = null;
                IEnumerable<IList<IFindReferenceSearchScope>> searchScopes = null;
                if (resolveResult is TypeResolveResult)
                {
                    var type = (resolveResult as TypeResolveResult).Type;
                    entity = type.GetDefinition();
                    ProcessTypeResults(type);
                    searchScopes = new[] { findReferences.GetSearchScopes(entity) };
                }

                if (resolveResult is MemberResolveResult)
                {
                    entity = (resolveResult as MemberResolveResult).Member;
                    if (entity.SymbolKind == SymbolKind.Constructor)
                    {
                        // process type instead
                        var type = entity.DeclaringType;
                        entity = entity.DeclaringTypeDefinition;
                        ProcessTypeResults(type);
                        searchScopes = new[] { findReferences.GetSearchScopes(entity) };
                    }
                    else
                    {
                        ProcessMemberResults(resolveResult);
                        var member = (resolveResult as MemberResolveResult).Member;
                        var members = MemberCollector.CollectMembers(_solution,
                                          member, false);
                        searchScopes = members.Select(findReferences.GetSearchScopes);
                    }
                }

                if (entity == null)
                    return _result;

                var projectsThatReferenceUsage = _projectFinder.FindProjectsReferencing(entity.Compilation.TypeResolveContext);

                foreach (var project in projectsThatReferenceUsage)
                {
                    var pctx = project.ProjectContent.CreateCompilation();
                    var interesting = (from file in project.Files
                                                      select (file.ParsedFile as CSharpUnresolvedFile)).ToList();

                    Parallel.ForEach(interesting.Distinct(), file =>
                        {
                            string text = _solution.GetFile(file.FileName).Content.Text;
                            SyntaxTree unit ;
                            if(project.CompilerSettings!=null){
                            	unit = new CSharpParser(project.CompilerSettings).Parse(text, file.FileName);
                            }else{
                            	unit = new CSharpParser().Parse(text, file.FileName);
                            }

                            foreach (var scope in searchScopes)
                            {
                                findReferences.FindReferencesInFile(scope, file, unit,
                                    pctx,
                                    (node, rr) => _result.Add(node.GetIdentifier()),
                                    CancellationToken.None);
                            }
                        });
                }
            }
            return _result;
        }

        private void ProcessMemberResults(ResolveResult resolveResult)
        {
            //TODO: why does FindReferencesInFile not return the definition for a field? 
            // add it here instead for now. 
            var definition = resolveResult.GetDefinitionRegion();
            ProcessRegion(definition);
        }

        private void ProcessRegion(DomRegion definition)
        {
            var file = _solution.GetFile(definition.FileName);
            if (file == null)
                return;
            var syntaxTree = file.SyntaxTree;
            var declarationNode = syntaxTree.GetNodeAt(definition.BeginLine, definition.BeginColumn);
            if (declarationNode != null)
            {
                declarationNode = FindIdentifier(declarationNode);

                if (IsIdentifier(declarationNode))
                    _result.Add(declarationNode);
            }
        }

        private static AstNode FindIdentifier(AstNode declarationNode)
        {
            while (declarationNode.GetNextNode() != null
                   && !(IsIdentifier(declarationNode)))
            {
                declarationNode = declarationNode.GetNextNode();
            }
            return declarationNode;
        }

        private void ProcessTypeResults(IType type)
        {
            //TODO: why does FindReferencesInFile not return the constructors?
            foreach (var constructor in type.GetConstructors())
            {
                var definition = constructor.MemberDefinition.Region;
                ProcessRegion(definition);
            }
        }

        private static bool IsIdentifier(AstNode declarationNode)
        {
            return declarationNode is VariableInitializer || declarationNode is Identifier;
        }
    }

    public class NodeComparer : IEqualityComparer<AstNode>
    {
        public bool Equals(AstNode x, AstNode y)
        {
            return x.StartLocation == y.StartLocation;
        }

        public int GetHashCode(AstNode obj)
        {
            return base.GetHashCode();
        }
    }
}
