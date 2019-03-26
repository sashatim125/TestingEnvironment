using System.Linq;
using Raven.Client.Documents.Indexes;
using TestingEnvironment.Common.Orchestrator;

namespace TestingEnvironment.Orchestrator
{
    public class LatestTestByName : AbstractIndexCreationTask<TestInfo, TestInfo>
    {
        public LatestTestByName()
        {
            Map = tests => from test in tests
                select new
                {
                    test.Start,
                    test.Name,
                    test.Config,
                    test.ExtendedName,
                    test.End,
                    test.Events
                };

            Reduce = results => from result in results
                group result by result.Name
                into g
                let lastResult = g.OrderBy(x => x.Start).Last()
                select new
                {
                    lastResult.Start,
                    lastResult.Name,
                    lastResult.Config,
                    lastResult.ExtendedName,
                    lastResult.End,
                    lastResult.Events
                };
        }
    }
}
