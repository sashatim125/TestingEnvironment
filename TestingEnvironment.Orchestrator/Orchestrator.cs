using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;
using Raven.Client.Extensions;
using Raven.Client.Http;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Commands;
using Raven.Client.ServerWide.Operations;
using Raven.Embedded;
using Sparrow.Json;
using TestingEnvironment.Common;

namespace TestingEnvironment.Orchestrator
{
    public class Orchestrator
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Lazy<Orchestrator> _instance = new Lazy<Orchestrator>(() => new Orchestrator());
        public static Orchestrator Instance => _instance.Value;

        //we need this - perhaps we would need to monitor server statuses? create/delete additional databases?
        private readonly DocumentStore _documentStore; 

        private readonly OrchestratorConfiguration _config;

        protected Orchestrator()
        {
            var configProvider = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional:false, reloadOnChange:true)
                .Build();
            
            _config = new OrchestratorConfiguration();
            ConfigurationBinder.Bind(configProvider, _config);

            foreach (var serverInfo in _config.RavenServers)
            {
                RaiseServer(serverInfo);
            }
            
            EmbeddedServer.Instance.StartServer();

            _documentStore = new DocumentStore
            {
                Database = _config.DefaultDatabase,
                Urls = GetUrls()
            };
            _documentStore.Initialize();

            EnsureDatabaseExists(_config.DefaultDatabase, truncateExisting:true);
        }

        private string[] GetUrls() => 
            _config.RavenServers.Select(x => $"http://{x.Url.Replace("http://",string.Empty).Replace("https://",string.Empty)}:{x.Port}").ToArray();

        public TestClientConfig RegisterTestClient(string testName)
        {
            //TODO: check if test name is unique
            //TODO: record client information in Raven embedded instance
            return new TestClientConfig
            {
                RavenUrls = GetUrls(),
                Database = _config.DefaultDatabase
            };
        }

        public EventResponse ReportEvent(string testName, EventInfo @event)
        {
            //TODO: check test names - are they unique?
            //TODO: record client event in Raven embedded instance (logging, reporting etc)

            return new EventResponse
            {
                Type = EventResponse.ResponseType.Ok //opportunity to request the test client to abort test...
            };            
        }
        
        private void EnsureDatabaseExists(string databaseName, bool truncateExisting = false)
        {
            var databaseNames = _documentStore.Maintenance.Server.Send(new GetDatabaseNamesOperation(0, int.MaxValue));
            if (truncateExisting && databaseNames.Contains(databaseName))
            {
                var result = _documentStore.Maintenance.Server.Send(new DeleteDatabasesOperation(databaseName, true));
                if (result.PendingDeletes.Length > 0)
                {
                    using (var ctx = JsonOperationContext.ShortTermSingleUse())
                        _documentStore.GetRequestExecutor()
                            .Execute(new WaitForRaftIndexCommand(result.RaftCommandIndex), ctx);
                }

                var doc = new DatabaseRecord(databaseName);
                _documentStore.Maintenance.Server.Send(new CreateDatabaseOperation(doc));

            }
            else if (!databaseNames.Contains(databaseName))
            {
                var doc = new DatabaseRecord(databaseName);
                _documentStore.Maintenance.Server.Send(new CreateDatabaseOperation(doc));
            }
        }        

        #region Raven Instance Activation Methods

        public void RaiseServer(ServerInfo server)
        {
            var args = new StringBuilder($@" --ServerUrl=http://0.0.0.0:{server.Port}");
            args.Append($@" --PublicServerUrl=http://{server.Url}:{server.Port}");
            args.Append($@" --License.Eula.Accepted=True");
            args.Append($@" --Security.UnsecuredAccessAllowed=PublicNetwork");
            args.Append($@" --Setup.Mode=None");

            var processStartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(server.Path, "Raven.Server.exe"),
                WorkingDirectory = server.Path,
                Arguments = args.ToString(),
                // CreateNoWindow = true,
                // RedirectStandardOutput = true,
                // RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false,                
            };
            
            var process = Process.Start(processStartInfo);
            //process.WaitForInputIdle(3000);

            //string url = null;
            //var outputString = ReadOutput(process.StandardOutput, async (line, builder) =>
            //{
            //    if (line == null)
            //    {
            //        var errorString = ReadOutput(process.StandardError, null);

            //        ShutdownServerProcess(process);

            //        throw new InvalidOperationException($"Failed to RaiseServer {server.Url}");
            //    }

            //    const string prefix = "Server available on: ";
            //    if (line.StartsWith(prefix))
            //    {
            //        url = line.Substring(prefix.Length);
            //        return true;
            //    }

            //    return false;
            //});
        }

        private static string ReadOutput(StreamReader output, Func<string, StringBuilder, Task<bool>> onLine)
        {
            var sb = new StringBuilder();

            var startupDuration = Stopwatch.StartNew();

            Task<string> readLineTask = null;
            while (true)
            {
                if (readLineTask == null)
                    readLineTask = output.ReadLineAsync();

                var hasResult = readLineTask.WaitWithTimeout(TimeSpan.FromSeconds(5)).Result;

                if (startupDuration.Elapsed > TimeSpan.FromSeconds(30))
                    return null;

                if (hasResult == false)
                    continue;

                var line = readLineTask.Result;

                readLineTask = null;

                if (line != null)
                    sb.AppendLine(line);

                var shouldStop = false;
                if (onLine != null)
                    shouldStop = onLine(line, sb).Result;

                if (shouldStop)
                    break;

                if (line == null)
                    break;
            }

            return sb.ToString();
        }

        private void ShutdownServerProcess(Process process)
        {
            if (process == null || process.HasExited)
                return;

            lock (process)
            {
                if (process.HasExited)
                    return;

                try
                {
                    Console.WriteLine($"Try shutdown server PID {process.Id} gracefully.");

                    using (var inputStream = process.StandardInput)
                    {
                        inputStream.Write($"q{Environment.NewLine}y{Environment.NewLine}");
                    }

                    if (process.WaitForExit((int) 30000))
                        return;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to shutdown server PID {process.Id} gracefully in 30Secs", e);
                }

                try
                {
                    Console.WriteLine($"Killing global server PID {process.Id}.");

                    process.Kill();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to kill process {process.Id}", e);
                }
            }
}        

        #endregion        
    }
}
