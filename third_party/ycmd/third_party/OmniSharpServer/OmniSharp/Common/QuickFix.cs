using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.TypeSystem;
using OmniSharp.Solution;

namespace OmniSharp.Common
{
    public class QuickFix
    {
        public string LogLevel { get; set; }

        private string _fileName;

        public string FileName 
        {
            get
            {
                return _fileName;
            }
            set { _fileName = value.ApplyPathReplacementsForClient(); } 
        }

        public int Line { get; set; }
        public int Column { get; set; }
        public int EndLine { get; set; }
        public int EndColumn { get; set; }
        public string Text { get; set; }

        /// <summary>
        ///   Initialize a QuickFix pointing to the first line of the
        ///   given region in the given file.
        /// </summary>
        public static QuickFix ForFirstLineInRegion
            (DomRegion region, CSharpFile file) {

            return QuickFix.ForFirstLineInRegion
                (region, file.Document);
        }

        public static QuickFix ForFirstLineInRegion
            (DomRegion region, IDocument document) {
            // Note that we could display an arbitrary amount of
            // context to the user: ranging from one line to tens,
            // hundreds..
            var text = document.GetText
                ( offset: document.GetOffset(region.BeginLine, column: 0)
                , length: document.GetLineByNumber
                            (region.BeginLine).Length)
                .Trim();

            return new QuickFix
                { FileName  = region.FileName
                , Line      = region.BeginLine
                , Column    = region.BeginColumn
                , EndLine   = region.EndLine
                , EndColumn = region.EndColumn
                , Text      = text};
        }

        /// <summary>
        ///   Creates a new QuickFix representing the non-bodyRegion
        ///   of the given region. Can be used to create QuickFixes
        ///   for AST members. The resulting QuickFix will then
        ///   contain the name and type signature of the member.
        /// </summary>
        /// <example>
        ///   For the region containing a "public string GetText(...)
        ///   {return null}" this method will return a QuickFix whose
        ///   Text is "public string GetText(...) ". So the returned
        ///   Text contains the type signature and not the body.
        /// </example>
        public static QuickFix ForNonBodyRegion
            (IUnresolvedEntity entity, IDocument document) {

            var text = GetNonBodyRegion
                (entity.Region, document, entity.BodyRegion);

            return new QuickFix
                { FileName  = entity.Region.FileName
                , Line      = entity.Region.BeginLine
                , Column    = entity.Region.BeginColumn
                , EndLine   = entity.Region.EndLine
                , EndColumn = entity.Region.EndColumn
                , Text      = text};

        }

        public static QuickFix ForNonBodyRegion
            (IMember member, IDocument document) {
            var text = GetNonBodyRegion
                (member.Region, document, member.BodyRegion);
            return new QuickFix
                { FileName = member.Region.FileName
                , Line     = member.Region.BeginLine
                , Column   = member.Region.BeginColumn
                , EndLine  = member.Region.EndLine
                , EndColumn = member.Region.EndColumn
                , Text     = text};
        }

        static string GetNonBodyRegion
            (DomRegion region, IDocument document, DomRegion bodyRegion) {

            // Delegates have no body, so they will crash if we don't do this
            if (bodyRegion.BeginLine == 0
                && bodyRegion.EndLine == 0)
                bodyRegion = region;

            var begin     = document.GetOffset(region.Begin);
            var bodyStart = document.GetOffset(bodyRegion.Begin);

            // Some entities have no body. These include members in
            // interfaces.
            bool hasNoBody = bodyStart == begin;
            var typeSignatureLength = hasNoBody
				? document.GetOffset(region.End) - begin
				: bodyStart - begin;
            // Note: We remove extra spaces and newlines from the type
            // signature to make displaying it easier in Vim. Other
            // editors might not have a problem with displaying
            // results with multiple lines.
            var text = document.GetText
                ( offset: document.GetOffset(region.Begin)
                , length: typeSignatureLength)
                .MultipleWhitespaceCharsToSingleSpace();

            return text;
        }
    }
}
