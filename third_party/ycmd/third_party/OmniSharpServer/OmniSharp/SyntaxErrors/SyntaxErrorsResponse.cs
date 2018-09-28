using System.Collections.Generic;
using OmniSharp.Common;

namespace OmniSharp.SyntaxErrors
{
    public class SyntaxErrorsResponse
    {
        public IEnumerable<Error> Errors { get; set; }
    }
}
