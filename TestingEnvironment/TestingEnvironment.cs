using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Raven.Client.Documents;

namespace TestingEnvironment
{
    public class ServerInfo
    {
        public string Url;
        public string Port;
        public string Path;
        public IDocumentStore[] DocumentStores;
    }    

    public class TestingEnvironment
    {
        private string LogFile = "TestingEnvironment.log";

        public void InitEnvironment(ServerInfo[] servers, int databaseCount)
        {                        
            var generator = new TestingEnvironmentGenerator();
            foreach (var server in servers)
            {
                Console.WriteLine($"Starting {server.Url}:{server.Port}");
                generator.RaiseServer(server);
                var stores = new DocumentStore[databaseCount];
                generator.CreateDatabases(server, stores, databaseCount);
                server.DocumentStores = stores;
            }
        }

        public void ReportFailure(string s, string stackTrace)
        {
            var txt = $"{DateTime.UtcNow}: {s}. StackTrace:{stackTrace}";
            File.AppendAllLines(LogFile, new [] {txt});
        }
    }
}
