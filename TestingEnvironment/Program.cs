using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Exceptions.Security;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;

namespace TestingEnvironment
{
    class Program
    {
        public static TestingEnvironment _te = new TestingEnvironment();

        const int StartPort = 8080;
        private const int NumberOfServers = 3;
        const int StoresPerServer = 3;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting TE...");
            var servers = new ServerInfo[NumberOfServers];
            CreateServersDatabasesAndGetStore(servers);

            DoScenario(servers);
        }

        private static void CreateServersDatabasesAndGetStore(ServerInfo[] servers)
        {            
            for (int i = StartPort; i < StartPort + servers.Length - 1; i++)
            {
                var s = new ServerInfo
                {
                    Url = @"http://127.0.0.1",
                    Port = i.ToString(),
                    Path = $@"D:\RavenDB_TE_{i - StartPort + 1}\Server"
                };
                servers[i - StartPort] = s;
            }
            
            _te.InitEnvironment(servers, StoresPerServer);
        }





        public class Car
        {
            public string Name;
        }

        private class Car_Name: AbstractIndexCreationTask<Car>
        {
            public Car_Name()
            {
                Map = cars => from car in cars
                    select new
                    {
                        car.Name
                    };
            }
        }


        private static void DoScenario(ServerInfo[] servers)
        {
            var renault = "Renault-" + Guid.NewGuid();
            var mazda = "Mazda-" + Guid.NewGuid();
            var jaguar = "Jaguar-" + Guid.NewGuid();

            using (var session = servers[0].DocumentStores[0].OpenSession())
            {
                session.Store(new Car
                {
                    Name = renault
                });

                session.Store(new Car
                {
                    Name = mazda
                });

                session.Store(new Car
                {
                    Name = jaguar
                });

                session.SaveChanges();
            }

            using (var session = servers[1].DocumentStores[0].OpenSession())
            {
                List<Car> cars = null;
                for (int retries = 0; retries < 3; retries++)
                {
                    cars = session.Query<Car, Car_Name>().Where(x => x.Name == mazda).ToList();
                    if (cars.Count != 1)
                        Thread.Sleep(TimeSpan.FromSeconds(10));
                }

                if (cars == null || cars.Count != 1)
                {
                    _te.ReportFailure($"Didn't got replicated car {mazda}", Environment.StackTrace);
                }

            }
    }
}
