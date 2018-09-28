
using System;
using System.IO;
using System.Text.RegularExpressions;
using OmniSharp.Common;
using OmniSharp.Configuration;
namespace OmniSharp.Solution
{
    public static class StringExtensions
    {

        /// <example>
        ///   "  " -> " ".
        ///   "foo   \n  bar" -> "foo bar".
        /// </example>
        public static string MultipleWhitespaceCharsToSingleSpace
            (this string stringToTrim) {
            return Regex.Replace(stringToTrim, @"\s+", " ");
        }

		public static string ForceWindowsPathSeparator(this string path)
		{
			return path.Replace ('/', '\\');
		}

		public static string ForceNativePathSeparator(this string path)
		{
			return path.Replace ('\\', Path.DirectorySeparatorChar);
		}

        public static string ForcePathSeparator(this string path, char directorySeparatorChar)
        {
            if (directorySeparatorChar == '/')
            {
                return path.Replace('\\', '/');
            }
            return path.Replace ('/', '\\');
        }

        /// <summary>
        /// Returns the relative path of a file to another file
        /// </summary>
        /// <param name="path">Base path to create relative path</param>
        /// <param name="pathToMakeRelative">Path of file to make relative against path</param>
        /// <returns></returns>
        public static string GetRelativePath(this string path, string pathToMakeRelative)
        {
            return new Uri(path).MakeRelativeUri(new Uri(pathToMakeRelative)).ToString().ForceWindowsPathSeparator();
        }

        public static string ApplyPathReplacementsForServer(this string path)
        {
            path = ApplyCygpathForServer(path);

            foreach (var pathReplacement in ConfigurationLoader.Config.PathReplacements)
            {
                path = path.Replace(pathReplacement.From, pathReplacement.To);
            }

            return path;
        }

        public static string ApplyPathReplacementsForClient(this string path)
        {
            path = ApplyCygpathForClient(path);

            foreach (var pathReplacement in ConfigurationLoader.Config.PathReplacements)
            {
                path = path.Replace(pathReplacement.To, pathReplacement.From);
            }

            return path;
        }

        private static string ApplyCygpathForServer(string path)
        {
            var config = ConfigurationLoader.Config;

            if (config.UseCygpath.GetValueOrDefault(config.ClientPathMode == PathMode.Cygwin))
            {
                path = CygPathWrapper.GetCygpath(path, config.ServerPathMode.Value);
            }

            return path;
        }

        private static string ApplyCygpathForClient(string path)
        {
            var config = ConfigurationLoader.Config;

            if (config.UseCygpath.GetValueOrDefault(config.ClientPathMode == PathMode.Cygwin))
            {
                path = CygPathWrapper.GetCygpath(path, config.ClientPathMode.GetValueOrDefault(PathMode.Unix));
            }

            return path;
        }
    }
}
