using ICSharpCode.NRefactory.CSharp;
using OmniSharp.Configuration;

namespace OmniSharp.CodeFormat
{
    public class CodeFormatHandler
    {
        readonly OmniSharpConfiguration _config;

        public CodeFormatHandler(OmniSharpConfiguration config)
        {
            _config = config;
        }
        public CodeFormatResponse Format(CodeFormatRequest request)
        {
            var options = _config.TextEditorOptions;
            var policy = _config.CSharpFormattingOptions;
            var formatter = new CSharpFormatter(policy, options);
            formatter.FormattingMode = FormattingMode.Intrusive;
            var output = formatter.Format(request.Buffer);
            return new CodeFormatResponse(output);
        }
    }
}
