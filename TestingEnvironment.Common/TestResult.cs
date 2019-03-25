using System;

namespace TestingEnvironment.Common
{
    public enum TestOutcome
    {
        Success = 1,
        Failure = 2
    }

    public class TestResult
    {
        public string Message { get; set; }
        public Exception Exception { get; set; }
        public TestOutcome Outcome { get; set; }
    }
}
