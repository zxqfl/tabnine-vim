using System;
using System.Diagnostics;

namespace OmniSharp
{
    public enum Verbosity
    {
        Silent,
        Quiet,
        Debug,
        Verbose
    }

    public class Logger
    {
        Verbosity _verbosity;
        public Verbosity Verbosity { get { return _verbosity; } }

        Stopwatch _stopwatch;

        public Logger(Verbosity verbosity)
        {
            _verbosity = verbosity;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        public void Debug(string message)
        {
            if (_verbosity != Verbosity.Quiet)
                Log(message);
        }

        public void Info(string message)
        {
            if (_verbosity == Verbosity.Verbose)
                Log(message);
        }

        public void Info(object message)
        {
            if (_verbosity == Verbosity.Verbose)
                Log(message);
        }

        public void Debug(string message, params object[] arg)
        {
            if (_verbosity != Verbosity.Quiet)
                Log(string.Format(message, arg));
        }

        public void Error(object message)
        {
            if (_verbosity != Verbosity.Silent)
            {
                var colour = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Log(message);
                Console.ForegroundColor = colour;
            }
        }

        private void Log(object message)
        {
            Console.WriteLine(_stopwatch.ElapsedMilliseconds.ToString("00000000 ") + message);
        }
    }
}