using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Raven.Client;
using Raven.Client.Documents;
using Raven.Client.Extensions;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;

namespace TestingEnvironment
{
    public class TestingEnvironmentGenerator
    {
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
                UseShellExecute = false
            };

            var process = Process.Start(processStartInfo);

            string url = null;
            var outputString = ReadOutput(process.StandardOutput, async (line, builder) =>
            {
                if (line == null)
                {
                    var errorString = ReadOutput(process.StandardError, null);

                    ShutdownServerProcess(process);

                    throw new InvalidOperationException($"Failed to RaiseServer {server.Url}");
                }

                const string prefix = "Server available on: ";
                if (line.StartsWith(prefix))
                {
                    url = line.Substring(prefix.Length);
                    return true;
                }

                return false;
            });
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

        public void CreateDatabases(ServerInfo server, IDocumentStore[] stores, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var dbname = $"Database" + i;
                var doc = new DatabaseRecord(dbname);
                var newstore = new DocumentStore
                {
                    Database = dbname,
                    Urls = new[] {server.Url + ":" + server.Port}
                }.Initialize();
                
                stores[i] = newstore;
                newstore.Maintenance.Server.Send(new CreateDatabaseOperation(doc));
            }
        }
    }
}
