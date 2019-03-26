namespace TestingEnvironment.Orchestrator
{
    public class ServerInfo
    {
        public string Port { get; set; }
        public string Path { get; set; }
        public string Url { get; set; }
    } 

    public class OrchestratorConfiguration
    {
        public string DefaultDatabase { get; set; }
        public ServerInfo[] RavenServers { get; set; }
        public string[] RemoteRavenServers { get; set; }
    }
}
