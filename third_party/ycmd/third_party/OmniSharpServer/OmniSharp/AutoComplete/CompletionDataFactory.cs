using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Completion;
using ICSharpCode.NRefactory.Completion;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using OmniSharp.Documentation;
using OmniSharp.Solution;
using OmniSharp.Configuration;

namespace OmniSharp.AutoComplete
{
    public class CompletionDataFactory : ICompletionDataFactory
    {
        private readonly string _partialWord;
        private readonly bool _instantiating;
        private readonly CSharpAmbience _ambience = new CSharpAmbience { ConversionFlags = AmbienceFlags };
        private readonly CSharpAmbience _signatureAmbience = new CSharpAmbience { ConversionFlags = AmbienceFlags | ConversionFlags.ShowReturnType | ConversionFlags.ShowBody };
        private readonly ReturnTypeAmbience _returnTypeAmbience = new ReturnTypeAmbience();

        private const ConversionFlags AmbienceFlags =
            ConversionFlags.ShowParameterList |
            ConversionFlags.ShowParameterNames;

        private string _completionText;
        private string _signature;
        private readonly bool _wantDocumentation;
        private readonly IProject _project;
        private bool _wantMethodHeader;
        private bool _wantSnippet;
        private bool _wantReturnType;
        private readonly OmniSharpConfiguration _config;

        public CompletionDataFactory(IProject project, string partialWord, bool instantiating, AutoCompleteRequest request, OmniSharpConfiguration config)
        {
            _project = project;
            _partialWord = partialWord;
            _instantiating = instantiating;
            _wantDocumentation = request.WantDocumentationForEveryCompletionResult;
            _wantMethodHeader = request.WantMethodHeader;
            _wantSnippet = request.WantSnippet;
            _wantReturnType = request.WantReturnType;
            _config = config;
        }

        public ICompletionData CreateImportCompletionData(IType type, bool useFullName, bool addForTypeCreation)
        {
            var result = CreateTypeCompletionData(type, useFullName, false, addForTypeCreation);
            Action<ICompletionData, int> setAsImport = null;
            setAsImport = (ICompletionData icompleteData, int depth) =>
            {
                if (depth > 5)
                    return;
                icompleteData.DisplayFlags |= DisplayFlags.IsImportCompletion;
                icompleteData.DisplayText += " [Using " + type.Namespace + "]";
                icompleteData.Description = "Using " + type.Namespace + "\n" + icompleteData.Description;
                var completeData = icompleteData as CompletionData;
                if (completeData != null)
                {
                    completeData.RequiredNamespaceImport = type.Namespace;
                }
                foreach (var overload in icompleteData.OverloadedData.Where(i => i != icompleteData))
                {
                    setAsImport(overload, depth + 1);
                }
            };
            setAsImport(result, 0);
            return result;
        }

        public ICompletionData CreateFormatItemCompletionData(string format, string description, object example)
        {
            throw new NotImplementedException();
        }

        public ICompletionData CreateXmlDocCompletionData(string tag, string description = null, string tagInsertionText = null)
        {
            throw new NotImplementedException();
        }

        public ICompletionData CreateEntityCompletionData(IEntity entity)
        {
            _completionText = _signature = entity.Name;

            _completionText = _ambience.ConvertSymbol(entity).TrimEnd(';');
            if (!_completionText.IsValidCompletionFor(_partialWord))
                return new CompletionData("~~");


            if (entity is IMethod)
            {
                var method = entity as IMethod;
                GenerateMethodSignature(method);
            }

            if (entity is IField || entity is IProperty)
            {
                _signature = _signatureAmbience.ConvertSymbol(entity).TrimEnd(';');
            }

            CompletionData completionData = CompletionData(entity) as CompletionData;

            if (entity is IMethod)
            {
                AddMethodHeader(completionData, entity as IMethod);
            }

            if (entity is IField || entity is IProperty)
            {
                if (_wantReturnType)
                {
                    completionData.ReturnType = _returnTypeAmbience.ConvertSymbol(entity);
                }

                if (_wantSnippet)
                {
                    completionData.Snippet = new SnippetGenerator(true).Generate(entity);
                }
            }
            Debug.Assert(completionData != null);
            return completionData;
        }

        private void AddMethodHeader(CompletionData completionData, IMethod method)
        {
            var methodDefinition = method.MemberDefinition;

            if (_wantMethodHeader)
            {
                var snippetGenerator = new SnippetGenerator(false);
                completionData.MethodHeader = snippetGenerator.Generate(methodDefinition);
            }

            if (_wantSnippet)
            {
                var snippetGenerator = new SnippetGenerator(true);
                completionData.Snippet = snippetGenerator.Generate(methodDefinition);
            }

            if (_wantReturnType)
            {
                completionData.ReturnType = _returnTypeAmbience.ConvertSymbol(method);
            }
        }

        private ICompletionData CompletionData(IEntity entity)
        {
            ICompletionData completionData;
            if (entity.Documentation != null)
            {
                completionData = new CompletionData(_signature, _completionText,
                    _signature + _config.TextEditorOptions.EolMarker +
                    DocumentationConverter.ConvertDocumentation(entity.Documentation, _config));
            }
            else
            {
                var ambience = new CSharpAmbience
                {
                    ConversionFlags = ConversionFlags.ShowParameterList |
                    ConversionFlags.ShowParameterNames |
                    ConversionFlags.ShowReturnType |
                    ConversionFlags.ShowBody |
                    ConversionFlags.ShowTypeParameterList
                };

                var documentationSignature = ambience.ConvertSymbol(entity);
                if (_wantDocumentation)
                {
                    string documentation = new DocumentationFetcher().GetDocumentation(_project, entity, _config);
                    var documentationAndSignature =
                        documentationSignature + _config.TextEditorOptions.EolMarker + documentation;
                    completionData = new CompletionData(_signature, _completionText, documentationAndSignature);
                }
                else
                {
                    completionData = new CompletionData(_signature, _completionText, documentationSignature);
                }
            }
            return completionData;
        }

        private IEnumerable<string> GetMethodParameterTypeNames(IMethod method)
        {
            foreach (var parameter in method.Parameters)
            {
                //TODO: this logic is far from complete. At the very least it needs some recursion
                if (parameter.Type is ArrayType)
                {
                    yield return (parameter.Type as ArrayType).ElementType.Name;
                }
                else if (parameter.Type is DefaultTypeParameter)
                {
                    yield return (parameter.Type as DefaultTypeParameter).Name;
                }
                else if (parameter.Type is ParameterizedType)
                {
                    foreach (var typeArgument in (parameter.Type as ParameterizedType).TypeArguments)
                    {
                        yield return typeArgument.Name;
                    }
                }
                else if (parameter.Type is UnknownType)
                {
                    yield return (parameter.Type as UnknownType).Name;
                }
            }
        }

        private bool MethodTypeParametersCanBeInferred(IMethod method)
        {
            if (method.IsExtensionMethod && method.Parameters.Count == 0)
            {
                // 'this' extension parameter is intentionally hidden by NRefactory
                // using ReducedExtensionMethod, so we can't check it
                return true;
            }
            var parameterTypes = GetMethodParameterTypeNames(method);
            var methodTypeParameters = method.TypeParameters.Select(p => p.FullName).Distinct();
            return !methodTypeParameters.Except(parameterTypes).Any();
        }

        private void GenerateMethodSignature(IMethod method)
        {
            _signature = _signatureAmbience.ConvertSymbol(method).TrimEnd(';');
            _completionText = _ambience.ConvertSymbol(method);
            _completionText = _completionText.Remove(_completionText.IndexOf('('));
            var parameterTypesCanBeInferred = MethodTypeParametersCanBeInferred(method);
            if ((method.TypeParameters.Count > 0 && method.TypeParameters[0].Name != "TSource") && !parameterTypesCanBeInferred)
            {
                _completionText += "<";
            }
            else
            {
                _completionText += "(";
                if (method.Parameters.Count == 0)
                {
                    _completionText += ")";
                }
            }
        }

        private void GenerateGenericMethodSignature(ISymbol method)
        {
            _signature = _signatureAmbience.ConvertSymbol(method).TrimEnd(';');
            _completionText = _signatureAmbience.ConvertSymbol(method);
            _completionText = _completionText.Remove(_completionText.IndexOf('(')) + "<";
        }

        public ICompletionData CreateEntityCompletionData(IEntity entity, string text)
        {
            return new CompletionData(text);
        }

        public ICompletionData CreateTypeCompletionData(IType type, bool showFullName, bool isInAttributeContext, bool addForTypeCreation)
        {
            if (!type.Name.IsValidCompletionFor(_partialWord))
            {
                return new CompletionData("~~");
            }
            CompletionData completion;
            if (addForTypeCreation || _instantiating)
            {
                completion = new CompletionData(type.Name);
                foreach (var constructor in type.GetConstructors())
                {
                    if (type.TypeParameterCount > 0)
                    {
                        GenerateGenericMethodSignature(constructor);
                        ICompletionData completionData = CompletionData(constructor);
                        AddMethodHeader(completionData as CompletionData, constructor);
                        completion.AddOverload(completionData);
                    }
                    else
                    {
                        var entityCompletionData = CreateEntityCompletionData(constructor);
                        AddMethodHeader(entityCompletionData as CompletionData, constructor);
                        completion.AddOverload(entityCompletionData);
                    }
                    AddMethodHeader(completion, constructor);
                }
            }
            else
            {
                var name = type.Name;
                if (type.TypeParameterCount > 0)
                {
                    name += "<";
                }
                completion = new CompletionData(name);
                completion.AddOverload(completion);
            }
            
            return completion;
        }

        public ICompletionData CreateMemberCompletionData(IType type, IEntity member)
        {
            return new CompletionData(type.Name);
        }

        public ICompletionData CreateLiteralCompletionData(string title, string description = null, string insertText = null)
        {
            return new CompletionData(title, description);
        }

        public ICompletionData CreateNamespaceCompletionData(INamespace name)
        {
            return new CompletionData(name.Name, name.FullName);
        }

        public ICompletionData CreateVariableCompletionData(IVariable variable)
        {
            var completionData = new CompletionData(variable.Name);
            if (_wantReturnType)
            {
                completionData.ReturnType = _returnTypeAmbience.ConvertSymbol(variable);
            }
            return completionData;
        }

        public ICompletionData CreateVariableCompletionData(ITypeParameter parameter)
        {
            return new CompletionData(parameter.Name);
        }

        public ICompletionData CreateEventCreationCompletionData(string varName, IType delegateType, IEvent evt,
                                                                 string parameterDefinition,
                                                                 IUnresolvedMember currentMember,
                                                                 IUnresolvedTypeDefinition currentType)
        {
            return new CompletionData(varName);
        }

        public ICompletionData CreateNewOverrideCompletionData(int declarationBegin, IUnresolvedTypeDefinition type,
                                                               IMember m)
        {
            return new CompletionData(m.Name);
        }

        public ICompletionData CreateNewPartialCompletionData(int declarationBegin, IUnresolvedTypeDefinition type,
                                                              IUnresolvedMember m)
        {
            return new CompletionData(m.Name);
        }

        public IEnumerable<ICompletionData> CreateCodeTemplateCompletionData()
        {
            return Enumerable.Empty<ICompletionData>();
        }

        public IEnumerable<ICompletionData> CreatePreProcessorDefinesCompletionData()
        {
            yield return new CompletionData("DEBUG");
            yield return new CompletionData("TEST");
        }

        public ICompletionData CreateImportCompletionData(IType type, bool useFullName)
        {
            return CreateImportCompletionData(type, useFullName, false);
        }
    }
}
