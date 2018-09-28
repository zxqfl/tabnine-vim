using System;
using System.Collections.Generic;
using System.Diagnostics;
using OmniSharp.Configuration;

namespace OmniSharp.Common
{
    public class CygPathWrapper
    {
        private static IDictionary<PathMode,Process> CygpathProcesses = new Dictionary<PathMode,Process>();
        private static IDictionary<PathMode,IDictionary<string,string>> CygpathCache = new Dictionary<PathMode,IDictionary<string,string>>();

        public static string GetCygpath(string path, PathMode target)
        {
            string convertedPath;

            if (!CygpathCache.ContainsKey(target))
            {
                CygpathCache[target] = new Dictionary<string,string>();
            }

            if (!CygpathCache[target].TryGetValue(path, out convertedPath))
            {
                lock (CygpathProcesses)
                {
                    Process process;
                    if (!CygpathProcesses.TryGetValue(target, out process) || process.HasExited)
                    {
                        var targetArgument = target == PathMode.Windows ? "-w"
                                           : target == PathMode.Unix ? "-u"
                                           : target == PathMode.Cygwin ? "-u"
                                           : string.Empty;
                        var processStartInfo = new ProcessStartInfo("cygpath", "-f - "+targetArgument)
                        {
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                        };

                        process = CygpathProcesses[target] = Process.Start(processStartInfo);
                    }

                    process.StandardInput.WriteLine(path);
                    convertedPath = process.StandardOutput.ReadLine();
                }
            }

            return convertedPath;
        }
    }
}
