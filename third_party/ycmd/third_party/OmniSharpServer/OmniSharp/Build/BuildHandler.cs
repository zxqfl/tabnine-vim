using System;
using System.Collections.Generic;
using System.Diagnostics;
using OmniSharp;
using OmniSharp.Common;

namespace OmniSharp.Build
{
    public class BuildHandler
    {
        private readonly BuildCommandBuilder _commandBuilder;
        private readonly BuildResponse _response;
        private readonly List<QuickFix> _quickFixes;
        private readonly BuildLogParser _logParser;
        private readonly Logger _logger;

        public BuildHandler(BuildCommandBuilder commandBuilder, Logger logger)
        {
            _commandBuilder = commandBuilder;
            _response = new BuildResponse();
            _logger = logger;
            _quickFixes = new List<QuickFix>();
            _logParser = new BuildLogParser();
        }

        public BuildResponse Build()
        {
            var build = _commandBuilder.Executable;

            var startInfo = new ProcessStartInfo
                {
                    FileName = build,
                    Arguments = _commandBuilder.BuildArguments(true),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

            var process = new Process
                {
                    StartInfo = startInfo,
                    EnableRaisingEvents = true
                };

            process.ErrorDataReceived += ErrorDataReceived;
            process.OutputDataReceived += OutputDataReceived;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();
            _response.QuickFixes = _quickFixes;

            return _response;
        }

        void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            _logger.Debug(e.Data);
            if (e.Data == null)
                return;

            //if (e.Data == "Build succeeded.")
             //   _response.Success = true;
            var quickfix = _logParser.Parse(e.Data);
            if(quickfix != null)
                _quickFixes.Add(quickfix);
        }

        void ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            _logger.Error(e.Data);
        }
    }
}
