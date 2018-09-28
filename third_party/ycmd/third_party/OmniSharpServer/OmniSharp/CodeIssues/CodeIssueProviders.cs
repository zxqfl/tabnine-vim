using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Refactoring;

namespace OmniSharp.CodeIssues
{
    public class CodeIssueProviders
    {
        public IEnumerable<CodeIssueProvider> GetProviders()
        {
			var types = Assembly.GetAssembly(typeof(IssueCategories))
                                .GetTypes()
                                .Where(t => typeof(CodeIssueProvider).IsAssignableFrom(t)
                                        && !t.IsAbstract);

            IEnumerable<CodeIssueProvider> providers =
                types
                    .Where(type => !type.IsInterface && !type.ContainsGenericParameters) //TODO: handle providers with generic params 
                    .Select(type => (CodeIssueProvider) Activator.CreateInstance(type));

            return providers;
        }
    }
}
