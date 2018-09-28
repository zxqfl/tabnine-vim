using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using OmniSharp.Common;
using OmniSharp.Configuration;
using Error = OmniSharp.Common.Error;
using OmniSharp.Solution;

namespace OmniSharp.SemanticErrors
{
    public class SemanticErrorsHandler
    {
        private readonly ISolution _solution;
        private readonly IEnumerable<string> _ignoredCodeIssues;

        public SemanticErrorsHandler(ISolution solution)
        {
            _solution = solution;
            _ignoredCodeIssues = ConfigurationLoader.Config.IgnoredCodeIssues;
        }

        public SemanticErrorsResponse FindSemanticErrors(Request request)
        {
            var clientFilename = request.FileName.ApplyPathReplacementsForClient();
            var project = _solution.ProjectContainingFile(request.FileName);
            project.UpdateFile(request.FileName, request.Buffer);
            var solutionSnapshot = new DefaultSolutionSnapshot(_solution.Projects.Select(i => i.ProjectContent));
            SyntaxTree syntaxTree;
            if(project.CompilerSettings!=null){
            	syntaxTree = new CSharpParser(project.CompilerSettings).Parse(request.Buffer, request.FileName);
            }else{
            	syntaxTree = new CSharpParser().Parse(request.Buffer, request.FileName);
            }
            var resolver = new CSharpAstResolver(solutionSnapshot.GetCompilation(project.ProjectContent), syntaxTree);
            var navigator = new SemanticErrorsNavigator();
            resolver.ApplyNavigator(navigator);
            var errors = navigator.GetErrors()
                .Where(e => ShouldIncludeIssue(e.Message))
                .Select(i => new Error
            {
                FileName = clientFilename,
                Message = i.Message,
                Line = i.StartLocation.Line,
                Column = i.EndLocation.Column,
                EndLine = i.EndLocation.Line,
                EndColumn = i.EndLocation.Column
            });

            return new SemanticErrorsResponse
            {
                Errors = errors,
            };
        }

        private bool ShouldIncludeIssue(string issue)
        {
            return !_ignoredCodeIssues.Any(ignore => Regex.IsMatch(issue, ignore));
        }

        struct ResolveErrors
        {
            public readonly TextLocation StartLocation;
            public readonly TextLocation EndLocation;
            public readonly String Message;

            public ResolveErrors(TextLocation startLocation, TextLocation endLocation, String message)
            {
                this.StartLocation = startLocation;
                this.EndLocation = endLocation;
                this.Message = message;
            }
        }

        class SemanticErrorsNavigator : IResolveVisitorNavigator
        {
            private readonly IList<ResolveErrors> _errors;

            public SemanticErrorsNavigator()
            {
                _errors = new List<ResolveErrors>();
            }

            public ResolveVisitorNavigationMode Scan(AstNode node)
            {
                return ResolveVisitorNavigationMode.Resolve;
            }

            public IList<ResolveErrors> GetErrors()
            {
                return _errors;
            }

            public void Resolved(AstNode node, ResolveResult resolveResult)
            {
                if (resolveResult.IsError)
                {
                    foreach(var error in GetErrorStrings(resolveResult))
                    {
                        _errors.Add(new ResolveErrors(node.StartLocation, node.EndLocation, error));
                    }
                }
            }

            public void ProcessConversion(Expression expression, ResolveResult resolveResult, Conversion conversion, IType type)
            {
                if (!conversion.IsValid)
                {
                    var error = String.Format("Cannot convert from {0} to {1}", resolveResult.Type, type);
                    _errors.Add(new ResolveErrors(expression.StartLocation, expression.EndLocation, error));
                }
            }

            public static IEnumerable<String> GetErrorStrings(ResolveResult resolveResult)
            {
                if (resolveResult is AwaitResolveResult)
                {
                    var specificResult = (AwaitResolveResult)resolveResult;
                    if (specificResult.GetAwaiterInvocation.IsError)
                    {
                        foreach(var error in GetErrorStrings(specificResult.GetAwaiterInvocation))
                        {
                            yield return error;
                        }
                    }
                }
                else if (resolveResult is CSharpInvocationResolveResult)
                {
                    var specificResult = (CSharpInvocationResolveResult)resolveResult;
                    foreach(var flag in Enum.GetValues(typeof(OverloadResolutionErrors)))
                    {
                        if (((OverloadResolutionErrors)flag & specificResult.OverloadResolutionErrors) != 0)
                        {
                            yield return String.Format("Invocation error: {0}", Enum.GetName(typeof(OverloadResolutionErrors), flag));
                        }
                    }
                }
                else if (resolveResult is ErrorResolveResult)
                {
                    var specificResult = (ErrorResolveResult)resolveResult;
                    if (!String.IsNullOrWhiteSpace(specificResult.Message))
                    {
                        yield return specificResult.Message;
                    }
                    else if (specificResult == ErrorResolveResult.UnknownError)
                    {
                        yield return "Unknown Resolver Error";
                    }
                    else
                    {
                        yield return String.Format("Error: {0}", specificResult.Type.Name);
                    }
                }
                else if (resolveResult is AmbiguousMemberResolveResult)
                {
                    var specificResult = (AmbiguousMemberResolveResult)resolveResult;
                    yield return String.Format("'{1}' contains ambiguous definitions for '{0}'", specificResult.Type.Name, specificResult.Member.Name);
                }
                else if (resolveResult is TypeResolveResult)
                {
                    var specificResult = (TypeResolveResult)resolveResult;
                    if (specificResult.Type == SpecialType.UnknownType)
                    {
                        yield return "Could not determine type";
                    }
                    else
                    {
                        yield return String.Format("'{0}' could not be resolved", specificResult.Type);
                    }
                }
                else if (resolveResult is UnknownMemberResolveResult)
                {
                    var specificResult = (UnknownMemberResolveResult)resolveResult;
                    yield return String.Format("'{0}' does not contain a definition for '{1}'", specificResult.TargetType.Name, specificResult.MemberName);
                }
                else if (resolveResult is ConversionResolveResult)
                {
                    var specificResult = (ConversionResolveResult)resolveResult;
                    yield return String.Format("Cannot convert from '{0}' to '{1}'", specificResult.Conversion, specificResult.Type);
                }
                else if (resolveResult is UnknownIdentifierResolveResult)
                {
                    var specificResult = (UnknownIdentifierResolveResult)resolveResult;
                    yield return String.Format("'{0}' is not a known identifier", specificResult.Identifier);
                }
                else if (resolveResult is SizeOfResolveResult)
                {
                    yield return String.Format("TODO: {0}", resolveResult);
                }
                else
                {
                    yield return String.Format("TODO: {0}", resolveResult);
                }
            }
        }
    }
}
