using System;

namespace CPPClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            RunCppClientTest(args[0]);
        }

        static void RunCppClientTest(string orchestratorUrl)
        {
            using (var client = new Test(orchestratorUrl, "CppClientTest"))
            {
                client.Initialize();
                client.RunTest();
            }

        }
    }
}
