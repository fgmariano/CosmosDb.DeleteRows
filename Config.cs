using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Cosmos.DeleteRows {
    public static class Config {

        public static string[] Environments(IConfiguration config) {

            return config
                .GetSection("CosmosDb:Environment")
                .GetChildren()
                .Select(_ => _.Key)
                .ToArray();
        }

        public static string[] Containers(IConfiguration config)
        {
            return config
                .GetSection("CosmosDb")
                .GetChildren()
                .Select(_ => _.Key)
                .Where(_ => _ != "Environment")
                .ToArray();
        }
    }
}