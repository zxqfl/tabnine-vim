namespace OmniSharp.AutoComplete
{
    public class AutoCompleteResponse
    {
        public AutoCompleteResponse() { } // for deserialisation

        public AutoCompleteResponse(CompletionData d)
        {
            DisplayText = d.DisplayText;
            CompletionText = d.CompletionText;
            Description = d.Description;
            RequiredNamespaceImport = d.RequiredNamespaceImport;
            MethodHeader = d.MethodHeader;
            ReturnType = d.ReturnType;
            Snippet = d.Snippet;
        }

        public string CompletionText { get; private set; }
        public string Description { get; private set; }
        public string DisplayText { get; private set; }
        public string RequiredNamespaceImport { get; private set; }
        public string MethodHeader { get; private set; }
        public string ReturnType { get; private set; }
        public string Snippet { get; private set; }
    }
}
