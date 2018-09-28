using System.IO;

namespace OmniSharp.Common
{
    public class NativeFileSystem : IFileSystem
    {
        public FileInfo GetFileInfo(string filename)
        {
            return new FileInfo(filename);
        }

		public string GetDirectoryName(string filename)
		{
			return Path.GetDirectoryName(filename);
		}
    }
}
