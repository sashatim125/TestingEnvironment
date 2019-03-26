using Raven.Client.Documents;
using TestingEnvironment.Common;
using TestingEnvironment.Common.OrchestratorReporting;

namespace TestingEnvironment.Orchestrator
{
    public interface ITestConfigSelectorStrategy
    {
        void Initialize(OrchestratorConfiguration configuration);

        string Name { get; } //must be unique!

        void OnBeforeRegisterTest(IDocumentStore store);
        void OnAfterUnregisterTest(TestInfo testInfo, IDocumentStore store);
        
        TestConfig GetNextTestConfig();
    }
}
