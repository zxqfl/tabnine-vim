using OmniSharp.Common;
using OmniSharp.Solution;
using System.Linq;

namespace OmniSharp.GotoFile
{
    public class GotoFileHandler
    {
        readonly ISolution _solution;

        public GotoFileHandler(ISolution solution) {
            _solution = solution;
        }

        public QuickFixResponse GetSolutionFiles()
        {
            var quickFixes = _solution.Projects
                .SelectMany(p => p.Files)
                .Select(f => new QuickFix
                        { FileName = f.FileName
                        , Line = 1
                        , Column = 1
                        , Text = f.FileName});
            return new QuickFixResponse(quickFixes);
        }

    }
}
