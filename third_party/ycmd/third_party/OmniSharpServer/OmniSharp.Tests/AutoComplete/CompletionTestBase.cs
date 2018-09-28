using System.Collections.Generic;
using System.Linq;
using OmniSharp.AutoComplete;

namespace OmniSharp.Tests.AutoComplete
{
    public class CompletionTestBase
    {
        protected IEnumerable<string> DisplayTextFor(string input, bool includeImportableTypes = false)
        {
            return CompletionsDataFor(input, includeImportableTypes).Select(c => c.DisplayText);
        }

        protected IEnumerable<string> CompletionsFor(string input, bool includeImportableTypes = false)
        {
            return CompletionsDataFor(input, includeImportableTypes).Select(c => c.CompletionText);
        }

        protected IEnumerable<string> MethodHeaderFor(string input, bool includeImportableTypes = false)
        {
            return CompletionsDataFor(input, includeImportableTypes).Select(c => c.MethodHeader);
        }

        protected IEnumerable<string> SnippetFor(string input, bool includeImportableTypes = false)
        {
            return CompletionsDataFor(input, includeImportableTypes).Select(c => c.Snippet);
        }

		protected IEnumerable<string> ReturnTypeFor(string input, bool includeImportableTypes = false)
        {
            return CompletionsDataFor(input, includeImportableTypes).Select(c => c.ReturnType);
        }

        protected IEnumerable<CompletionData> CompletionsDataFor(string input, bool includeImportableTypes = false)
        {
            return new CompletionsSpecBase().GetCompletions(input, includeImportableTypes);
        }
    }
}