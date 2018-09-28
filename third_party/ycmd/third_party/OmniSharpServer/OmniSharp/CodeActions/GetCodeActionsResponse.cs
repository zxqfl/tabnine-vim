using System.Collections.Generic;

namespace OmniSharp.CodeActions
{
    public class GetCodeActionsResponse
    {
        public IEnumerable<string> CodeActions { get; set; } 
    }
}