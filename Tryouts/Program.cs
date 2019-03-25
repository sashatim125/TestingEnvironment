using TestingEnvironment.Client;
using TestingEnvironment.Common;

namespace Tryouts
{
    class Program
    {
        public class TestClient : BaseClient
        {
            public TestClient(string orchestratorUrl, string testName) : base(orchestratorUrl, testName)
            {
            }
       
            public override void RunTest()
            {
                ReportEvent(new EventInfo
                {
                    Message = "Hello World!"
                });
            }
        }
        static void Main(string[] args)
        {
            using (var client = new TestClient("http://localhost:5000", "MegaTesT!"))
            {
                client.Initialize();
                client.RunTest();
            }
        }
    }
}
