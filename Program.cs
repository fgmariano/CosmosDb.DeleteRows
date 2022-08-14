using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Cosmos.DeleteRows
{
    class Program
    {
        private IConfiguration _config;

        private void Run(string environment, string container)
        {
            var repo = new RepositoryBase(
                _config[$"CosmosDb:Environment:{environment}:ConnectionString"]
                , _config[$"CosmosDb:Environment:{environment}:DatabaseName"]
                , _config[$"CosmosDb:Environment:{environment}:ApplicationName"]
                , int.Parse(_config[$"CosmosDb:Environment:{environment}:CommandTimeout"])
                , _config[$"CosmosDb:{container}:ContainerName"]
                , _config[$"CosmosDb:{container}:PartitionKey"]);

            var items = repo.QueryAsync<JObject>($"SELECT * FROM e").Result;

            Console.WriteLine($"Deleting {items.Count} records");
            var teste = repo.BulkDeleteAsync(items.ToList()).Result;
            Console.WriteLine("Deleted");
        }

        static void Main(string[] args)
        {
            var e = new Program();
            string environment = "";
            string container = "";
            bool selectEnv = true;
            int response;

            var envs = Config.Environments(e._config);
            var containers = Config.Containers(e._config);

            while (true)
            {
                while (selectEnv)
                {
                    Console.WriteLine("Choose an environment:");
                    for (int i = 0; i < envs.Length; i++)
                    {
                        Console.WriteLine($"{i} - {envs[i]}");
                    }
                    response = int.Parse(Console.ReadKey().KeyChar.ToString());
                    Console.WriteLine("");
                    if (response >= envs.Length)
                        Console.WriteLine("Invalid option");
                    else
                        environment = envs[response]; break;
                }

                while (true)
                {
                    Console.WriteLine("Choose a container:");
                    for (int i = 0; i < containers.Length; i++)
                    {
                        Console.WriteLine($"{i} - {containers[i]}");
                    }
                    response = int.Parse(Console.ReadKey().KeyChar.ToString());
                    Console.WriteLine("");
                    if (response >= containers.Length)
                        Console.WriteLine("Invalid option");
                    else
                        container = containers[response]; break;
                }

                e.Run(environment, container);

                Console.WriteLine("");
                Console.WriteLine("1 - Choose a different environment");
                Console.WriteLine("2 - Choose a different container");
                Console.WriteLine("Hit any other key to end the process");
                Console.WriteLine("");

                response = int.Parse(Console.ReadKey().KeyChar.ToString());
                if (response == 1)
                    selectEnv = true;
                else if (response == 2)
                    selectEnv = false;
                else
                    break;
            }
        }

        public Program()
        {
            _config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
        }
    }
}
