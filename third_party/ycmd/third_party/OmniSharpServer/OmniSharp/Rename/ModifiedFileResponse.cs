using OmniSharp.Solution;

namespace OmniSharp.Rename
{
    public class ModifiedFileResponse
    {
        private string _fileName;
        public ModifiedFileResponse() {}

        public ModifiedFileResponse(string fileName, string buffer) {
            this.FileName = fileName;
            this.Buffer = buffer;
        }

        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value.ApplyPathReplacementsForClient();
            }
        }
        public string Buffer { get; set; }

    }
}
