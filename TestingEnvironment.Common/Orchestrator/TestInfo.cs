using System;
using System.Collections.Generic;

namespace TestingEnvironment.Common.Orchestrator
{
    public class TestInfo
    {
        public string Name { get; set; }
        public string ExtendedName { get; set; }
        public DateTime Start { get;set; }
        public DateTime End { get;set; }
        public List<EventInfo> Events { get;set; }
        public TestConfig Config { get; set; }
    }
}
