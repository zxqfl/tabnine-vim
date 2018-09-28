using System.IO;
using OmniSharp.Common;

namespace OmniSharp.Tests
{
	public class FakeWindowsFileSystem : FakeFileSystem
	{
		public FakeWindowsFileSystem () : base(@"\") {}
	}

	public class FakeUnixFileSystem : FakeFileSystem
	{
		public FakeUnixFileSystem () : base("/") {}
	}

    public class FakeFileSystem : IFileSystem
    {
		private string _pathSeparator;
		private bool _exists = true;

		public FakeFileSystem ()
		{
			_pathSeparator = @"\";	
		}

		public FakeFileSystem (string pathSeparator)
		{
			_pathSeparator = pathSeparator;
		}

		public string GetDirectoryName(string filename)
		{
			return filename.Substring (0, filename.LastIndexOf (_pathSeparator, System.StringComparison.Ordinal));
		}

        public void FileExists(bool exists)
        {
            _exists = exists;
        }

        public FileInfo GetFileInfo(string filename)
        {
            if(_exists)
                return new FileInfo("Nancy.dll");

            return new FileInfo("IDontExist.dll");
        }
    }
}