using System.Linq;
using Raven.Client.Documents.Indexes;
using TestingEnvironment.Common.OrchestratorReporting;

namespace TestingEnvironment.Orchestrator
{
    public class LatestTestByName : AbstractIndexCreationTask<TestInfo, TestInfo>
    {
        public LatestTestByName()
        {
            Map = tests => from test in tests
                select new
                {
                    test.Id,
                    test.Start,
                    test.Name,
                    test.Config,
                    test.Author,
                    test.ExtendedName,
                    test.TestClassName,
                    test.End,
                    test.Events
                };

            Reduce = results => from result in results
                group result by result.Name
                into g
                let lastResult = g.OrderBy(x => x.Start).Last()
                select new
                {
                    lastResult.Id,
                    lastResult.Start,
                    lastResult.Name,
                    lastResult.Config,
                    lastResult.Author,
                    lastResult.ExtendedName,
                    lastResult.TestClassName,
                    lastResult.End,
                    lastResult.Events
                };
        }
    }
}
