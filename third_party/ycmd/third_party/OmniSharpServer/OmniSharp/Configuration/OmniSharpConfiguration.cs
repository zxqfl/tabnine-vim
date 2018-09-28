using System.Collections.Generic;
using ICSharpCode.NRefactory.CSharp;

namespace OmniSharp.Configuration
{
    public class OmniSharpConfiguration
    {
        public OmniSharpConfiguration()
        {
            PathReplacements = new List<PathReplacement>();
            Defines = new List<string>();
            IgnoredCodeIssues = new List<string>();
            TextEditorOptions = new TextEditorOptions ();
            TextEditorOptions.TabsToSpaces = true;
            CSharpFormattingOptions = FormattingOptionsFactory.CreateAllman();
        }

        public IEnumerable<PathReplacement> PathReplacements { get; set; }
        public IEnumerable<string> Defines { get; set; }
        public IEnumerable<string> IgnoredCodeIssues { get; set; }
        public string MSBuildPath { get; set; }
        public TextEditorOptions TextEditorOptions { get; set; }
        public TestCommands TestCommands { get; set; }
        public string CSharpFormattingOptionsName { get; set; }
        public string ConfigFileLocation { get; set; }
        private CSharpFormattingOptions _options;
        public CSharpFormattingOptions CSharpFormattingOptions
        {
            set
            {
                switch (CSharpFormattingOptionsName)
                {
                    case "KRStyle":
                        _options = FormattingOptionsFactory.CreateKRStyle();
                        break;
                    case "Allman":
                        _options = FormattingOptionsFactory.CreateAllman();
                        break;
                    case "Empty":
                        _options = FormattingOptionsFactory.CreateEmpty();
                        break;
                    case "GNU":
                        _options = FormattingOptionsFactory.CreateGNU();
                        break;
                    case "Mono":
                        _options = FormattingOptionsFactory.CreateMono();
                        break;
                    case "SharpDevelop":
                        _options = FormattingOptionsFactory.CreateSharpDevelop();
                        break;
                    case "Whitesmiths":
                        _options = FormattingOptionsFactory.CreateWhitesmiths();
                        break;
                    default:
                        _options = value;
                        break;
                }
            }
            get
            {
                return _options;
            }
        }
        public bool? UseCygpath { get; set; }
        public PathMode? ClientPathMode { get; set; }
        public PathMode? ServerPathMode { get; set; }

        public class PathReplacement
        {
            public string From { get; set; }
            public string To { get; set; }
        }
    }
}
