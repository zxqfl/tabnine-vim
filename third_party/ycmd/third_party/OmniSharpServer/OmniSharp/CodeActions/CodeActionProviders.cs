using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ICSharpCode.NRefactory.CSharp.Refactoring;

namespace OmniSharp.CodeActions
{
    public class CodeActionProviders
    {
        public IEnumerable<CodeActionProvider> GetProviders()
        {
            var types = Assembly.GetAssembly(typeof(UseVarKeywordAction))
                                .GetTypes()
                                .Where(t => typeof(CodeActionProvider).IsAssignableFrom(t));

            IEnumerable<CodeActionProvider> providers =
                types
                    .Where(type => !type.IsInterface 
                            && !type.IsAbstract
                            && !type.ContainsGenericParameters) //TODO: handle providers with generic params 
                    .Select(type => (CodeActionProvider) Activator.CreateInstance(type));

            return providers;
        }
    }
}
