using OmniSharp.Rename;

namespace OmniSharp.Overrides {
    public class RunOverrideTargetResponse : ModifiedFileResponse {
        public RunOverrideTargetResponse() {}
        public RunOverrideTargetResponse
            ( string fileName
            , string buffer
            , int    line
            , int    column)
            : base(fileName: fileName, buffer: buffer) {
            this.Line = line;
            this.Column = column;
        }

        public int Line {get; set;}
        public int Column {get; set;}

    }
}
