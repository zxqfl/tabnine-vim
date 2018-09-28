using OmniSharp.Common;

namespace OmniSharp.LookupAllTypes
{
    public class LookupAllTypesRequest : Request
    {
        private bool _includeTypesWithoutSource = true;

        public bool IncludeTypesWithoutSource
        {
            get { return _includeTypesWithoutSource; }
            set { _includeTypesWithoutSource = value; }
        }
    }
}