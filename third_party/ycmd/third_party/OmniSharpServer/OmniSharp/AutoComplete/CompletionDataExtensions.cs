using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.Completion;

namespace OmniSharp.AutoComplete
{
    public static class CompletionDataExtensions
    {
        public static IEnumerable<CompletionData> FlattenOverloads(this IEnumerable<CompletionData> completions)
        {
            var res = new List<CompletionData>();
            foreach (var completion in completions)
            {
                res.AddRange(completion.HasOverloads ? completion.OverloadedData.Cast<CompletionData>() : new[] { completion });
            }
            return res;
        }

        public static IEnumerable<CompletionData> RemoveDupes(this IEnumerable<CompletionData> data)
        {
            return data.GroupBy(x => x.DisplayText,
                                (k, g) => g.Aggregate((a, x) => (CompareTo(x, a) == -1) ? x : a));
        }

        private static int CompareTo(ICompletionData a, ICompletionData b)
        {
            if (a.CompletionCategory == null && b.CompletionCategory == null)
                return 0;
            if (a.CompletionCategory == null)
                return -1;
            if (b.CompletionCategory == null)
                return 1;
            return a.CompletionCategory.CompareTo(b.CompletionCategory);
        }
    }
}
