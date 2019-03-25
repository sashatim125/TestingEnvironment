using Nancy;
using Nancy.ModelBinding;
using TestingEnvironment.Common;

// ReSharper disable VirtualMemberCallInConstructor

namespace TestingEnvironment.Orchestrator
{    
    public class OrchestratorController : NancyModule
    {
        private readonly OrchestratorConfiguration _config;

        public OrchestratorController(OrchestratorConfiguration config)
        {
            _config = config;
            Put("/register", @params => 
                Orchestrator.Instance.RegisterTestClient((string) @params.testName));

            Post("/report", 
                @params => Orchestrator.Instance.ReportEvent(@params.testName, this.Bind<EventInfo>()));
        }
    }
}
