using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using OmniSharp.Documentation;
using OmniSharp.Parser;
using OmniSharp.Solution;
using OmniSharp.Configuration;

namespace OmniSharp.TypeLookup
{
    public class TypeLookupHandler
    {
        private readonly BufferParser _bufferParser;

        private static readonly ConversionFlags AmbienceFlags =
            ConversionFlags.ShowBody |
            ConversionFlags.ShowModifiers |
            ConversionFlags.ShowReturnType |
            ConversionFlags.ShowParameterList |
            ConversionFlags.ShowParameterNames |
            ConversionFlags.ShowDeclaringType;

        private readonly ISolution _solution;

        private readonly OmniSharpConfiguration _config;

        public TypeLookupHandler(ISolution solution, BufferParser bufferParser, OmniSharpConfiguration config)
        {
            _config = config;
            _bufferParser = bufferParser;
            _solution = solution;
        }

        public TypeLookupResponse GetTypeLookupResponse(TypeLookupRequest request)
        {
            var res = _bufferParser.ParsedContent(request.Buffer, request.FileName);
            var loc = new TextLocation(request.Line, request.Column);
            var resolveResult = ResolveAtLocation.Resolve(res.Compilation, res.UnresolvedFile, res.SyntaxTree, loc);
            var response = new TypeLookupResponse();
            var ambience = new CSharpAmbience()
            {
                ConversionFlags = AmbienceFlags,
            };


            if (resolveResult == null || resolveResult is NamespaceResolveResult)
                response.Type = "";
            else
            {
                response.Type = resolveResult.Type.ToString();
                IEntity entity = null;
                if (resolveResult is CSharpInvocationResolveResult)
                {
                    var result = resolveResult as CSharpInvocationResolveResult;
                    entity = result.Member;
                    response.Type = ambience.ConvertSymbol(result.Member);
                }
                else if (resolveResult is LocalResolveResult)
                {
                    var result = resolveResult as LocalResolveResult;
                    response.Type = ambience.ConvertVariable(result.Variable);
                }
                else if (resolveResult is MemberResolveResult)
                {
                    var result = resolveResult as MemberResolveResult;
                    entity = result.Member;
                    response.Type = ambience.ConvertSymbol(result.Member);
                }
                else if (resolveResult is TypeResolveResult)
                {
                    ambience.ConversionFlags |= ConversionFlags.UseFullyQualifiedTypeNames;
                    response.Type = ambience.ConvertType(resolveResult.Type);
                }

                if (resolveResult.Type is UnknownType)
                    response.Type = "Unknown Type: " + resolveResult.Type.Name;
                if (resolveResult.Type == SpecialType.UnknownType)
                    response.Type = "Unknown Type";

                if (request.IncludeDocumentation && entity != null)
                {
                    var project = _solution.ProjectContainingFile(request.FileName);
                    response.Documentation = new DocumentationFetcher().GetDocumentation(project, entity, _config);
                }
            }

            return response;
        }
    }
}
