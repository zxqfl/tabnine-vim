using System.Collections.Generic;
using OmniSharp.Common;

namespace OmniSharp.SemanticErrors
{
    public class SemanticErrorsResponse
    {
        public IEnumerable<Error> Errors { get; set; }
    }
}
