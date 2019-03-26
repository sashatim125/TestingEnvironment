using System;
using System.Collections.Generic;
using System.Net;
using Raven.Client.Documents;
using ServiceStack;
using TestingEnvironment.Common;

namespace TestingEnvironment.Client
{
    public abstract class BaseTest : IDisposable
    {
        protected readonly string OrchestratorUrl;
        protected readonly string TestName;
        protected IDocumentStore DocumentStore;

        private readonly JsonServiceClient _orchestratorClient;

        protected BaseTest(string orchestratorUrl, string testName)
        {
            OrchestratorUrl = orchestratorUrl ?? throw new ArgumentNullException(nameof(orchestratorUrl));
            TestName = testName ?? throw new ArgumentNullException(nameof(testName));
            _orchestratorClient = new JsonServiceClient(OrchestratorUrl);
        }

        public virtual void Initialize()
        {            
            var config = _orchestratorClient.Put<TestClientConfig>($"/register?testName={TestName}",null);
            DocumentStore = new DocumentStore
            {
                Urls = config.RavenUrls,
                Database = config.Database
            };
            DocumentStore.Initialize();
        }

        public abstract void RunTest();

        protected void ReportSuccess(string message, Dictionary<string, string> additionalInfo = null)
        {
            ReportEvent(new EventInfo
            {
                Message = message,
                AdditionalInfo = additionalInfo,
                Type = EventInfo.EventType.TestSuccess
            });
        }

        protected void ReportFailure(string message, Exception error, Dictionary<string, string> additionalInfo = null)
        {
            ReportEvent(new EventInfo
            {
                Message = message,
                AdditionalInfo = additionalInfo,
                Exception = error,
                Type = EventInfo.EventType.TestFailure
            });
        }

        protected virtual EventResponse ReportEvent(EventInfo eventInfo) => 
            _orchestratorClient.Post<EventResponse>($"/report?testName={TestName}", eventInfo);

        public virtual void Dispose()
        {
            _orchestratorClient.Put<object>($"/unregister?testName={TestName}",null);

            _orchestratorClient.Dispose();
            DocumentStore.Dispose();
        }
    }
}
