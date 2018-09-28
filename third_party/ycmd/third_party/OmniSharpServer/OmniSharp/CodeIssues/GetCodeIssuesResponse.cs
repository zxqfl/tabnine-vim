using System.Collections.Generic;

namespace OmniSharp.CodeIssues
{
    public class GetCodeIssuesResponse
    {
        public IEnumerable<string> CodeActions { get; set; } 
    }
}