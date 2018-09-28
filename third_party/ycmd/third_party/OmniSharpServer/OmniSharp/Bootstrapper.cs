using System;
using System.Diagnostics;
using System.IO;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Helpers;
using Nancy.Json;
using Nancy.TinyIoc;
using MonoDevelop.Projects;
using OmniSharp.Common;
using OmniSharp.Configuration;
using OmniSharp.ProjectManipulation.AddReference;
using OmniSharp.Solution;

namespace OmniSharp
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        private readonly ISolution _solution;

        readonly IFileSystem _fileSystem;
        readonly Logger _logger;

        public Bootstrapper(ISolution solution, IFileSystem fileSystem, Logger logger)
        {
            _logger = logger;
            _fileSystem = fileSystem;
            _solution = solution;
            JsonSettings.MaxJsonLength = int.MaxValue;
			// so I don't break existing clients after Nancy upgrade
			JsonSettings.RetainCasing = true;
            StaticConfiguration.DisableErrorTraces = false;
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            if (PlatformService.IsUnix)
            {
                HelpService.AsyncInitialize();
            }

            pipelines.BeforeRequest += (ctx) => StopWatchStart (ctx);
            pipelines.AfterRequest += (ctx) => StopWatchStop (ctx);

            if (_logger.Verbosity == Verbosity.Verbose)
            {
                pipelines.BeforeRequest += (ctx) => LogRequest (ctx);
                pipelines.AfterRequest += (ctx) => LogResponse (ctx);
            }

            pipelines.OnError.AddItemToEndOfPipeline((ctx, ex) =>
                {
                    _logger.Error(ex);
                    return null;
                });
        }

        private Response LogRequest(NancyContext ctx)
        {
            _logger.Debug("************ Request ************");
            _logger.Debug("{0} - {1}", ctx.Request.Method, ctx.Request.Path);
            _logger.Debug("************ Headers ************");

            foreach(var headerGroup in ctx.Request.Headers)
            {
                foreach(var header in headerGroup.Value)
                {
                    _logger.Debug("{0} - {1}", headerGroup.Key, header);
                }
            }

            _logger.Debug("************  Body ************");
            using (var reader = new StreamReader(ctx.Request.Body))
            {
                var content = reader.ReadToEnd();
                _logger.Debug(HttpUtility.UrlDecode(content));
            }

            ctx.Request.Body.Position = 0;

            return null;
        }

        private void LogResponse(NancyContext ctx)
        {
            _logger.Debug("************  Response ************ ");

            var stream = new MemoryStream();
            
            ctx.Response.WithHeader("Access-Control-Allow-Origin", "*")
                                        .WithHeader("Access-Control-Allow-Methods", "POST,GET")
                                        .WithHeader("Access-Control-Allow-Headers", "Accept, Origin, Content-type");
            ctx.Response.Contents.Invoke(stream);

            stream.Position = 0;
            using (var reader = new StreamReader(stream))
            {
                var content = reader.ReadToEnd();
                _logger.Debug(content);
            }
        }

        private Response StopWatchStart(NancyContext ctx)
        {
            var stopwatch = new Stopwatch();
            ctx.Items["stopwatch"] = stopwatch;
            stopwatch.Start();
            return null;
        }

        private void StopWatchStop(NancyContext ctx)
        {
            if(ctx.Items.ContainsKey("stopwatch"))
            {
                var stopwatch = (Stopwatch) ctx.Items["stopwatch"];
                stopwatch.Stop();
                _logger.Debug(ctx.Request.Path + " " + stopwatch.ElapsedMilliseconds + "ms");
            }
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            container.Register(_solution);
            container.Register(_fileSystem);
            container.Register(_logger);
            container.Register(ConfigurationLoader.Config);
            container.RegisterMultiple<IReferenceProcessor>(new []{typeof(AddProjectReferenceProcessor), typeof(AddFileReferenceProcessor), typeof(AddGacReferenceProcessor)});			
        }
    }
}
