using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Runtime.InteropServices;
using TestingEnvironment.Client;

namespace CPPClientTest
{
    class Test : BaseTest
    {
        public Test(string orchestratorUrl, string testName) : base(orchestratorUrl, testName, "Alexander")
        {
        }

        [DllImport(@"test4.2.dll", EntryPoint = "run_tests")]
        private static extern unsafe void run_tests(
            string[] urls,
            Int32 numOfUrls,
            string dbName,
            void* resultBuffer,
            out Int32 resultBufferLength);

        private unsafe void RunMyTest()
        {
            var resultBuffer = new byte[1024 * 1024];
            Int32 resultBufferLength;

            ReportInfo("Cpp client test started");

            fixed (byte* buffer = resultBuffer)
            {
                run_tests(DocumentStore.Urls, DocumentStore.Urls.Length,
                    DocumentStore.Database,
                    buffer, out resultBufferLength);
            }

            var testResult = Encoding.UTF8.GetString(resultBuffer, 0, resultBufferLength);
            ReportInfo(testResult);

            ReportInfo("Cpp client test finished");
        }

        public override void RunActualTest()
        {
            RunMyTest();
        }
    }
}

