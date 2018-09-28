using OmniSharp.Common;

namespace OmniSharp.CodeActions
{
    public class RunCodeActionRequest : Request
    {
        public int CodeAction { get; set; }
    }
}
