using System.Data.SqlTypes;
using Nancy;
using Nancy.ModelBinding;
using TestingEnvironment.Common;

// ReSharper disable VirtualMemberCallInConstructor

namespace TestingEnvironment.Orchestrator
{    
    public class OrchestratorController : NancyModule
    {
        private readonly OrchestratorConfiguration _config;
        private static readonly object Empty = new object();

        public OrchestratorController(OrchestratorConfiguration config)
        {
            _config = config;
            Put("/register", @params => 
                Orchestrator.Instance.RegisterTest((string) Request.Query.testName));

            Put("/unregister", @params =>
            {
                Orchestrator.Instance.UnregisterTest((string) Request.Query.testName);
                return Empty;
            });

            Post("/report", 
                @params => Orchestrator.Instance.ReportEvent(Request.Query.testName, this.Bind<EventInfo>()));
        }
    }
}
