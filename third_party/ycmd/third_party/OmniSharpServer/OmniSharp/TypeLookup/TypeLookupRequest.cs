using OmniSharp.Common;

namespace OmniSharp.TypeLookup
{
    public class TypeLookupRequest : Request
    {
		public bool IncludeDocumentation
		{
			get
			{
				return includeDocumentation;
			}
			set
			{
				includeDocumentation = value;
			}
		}
        bool includeDocumentation = true;
    }
}
