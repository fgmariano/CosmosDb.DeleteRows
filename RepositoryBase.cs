using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Cosmos.DeleteRows
{
    public class RepositoryBase
    {
        private readonly string _partitionKey;
        private CosmosClient _client;
        private Container _container;

        public RepositoryBase(string connString, string database, string application, int commandTimeout, string container, string partitionKey)
        {
            _client = new CosmosClient(
                connString,
                new CosmosClientOptions()
                {
                    ApplicationName = application,
                    SerializerOptions = new CosmosSerializationOptions
                    {
                        PropertyNamingPolicy = CosmosPropertyNamingPolicy.Default,
                        IgnoreNullValues = true
                    },
                    RequestTimeout = TimeSpan.FromMinutes(commandTimeout),
                    AllowBulkExecution = true
                });

            _container = _client.GetContainer(database, container);
            _partitionKey = partitionKey;
        }

        public async Task<IList<T>> QueryAsync<T>(string sql, object parameters = null)
        {
            var itensList = new List<T>();

            QueryDefinition queryDefinition = GetQueryDefinition(sql, parameters);

            var queryResultSetIterator = _container.GetItemQueryIterator<T>(queryDefinition);

            if (!queryResultSetIterator.HasMoreResults)
                return default;

            while (queryResultSetIterator.HasMoreResults)
            {
                var resultItem = await queryResultSetIterator.ReadNextAsync();
                foreach (var item in resultItem)
                {
                    itensList.Add((T)item);
                }
            }

            return itensList;
        }

        public async Task<bool> BulkDeleteAsync(List<JObject> arr)
        {
            var threads = new List<Task>();
            foreach (var item in arr)
            {
                threads.Add(_container.DeleteItemAsync<JObject>((string)item["id"], new PartitionKey((string)item[_partitionKey])));
            }

            await Task.WhenAll(threads);
            return true;
        }

        private QueryDefinition GetQueryDefinition(string sql, object parameters = null)
        {
            QueryDefinition queryDefinition = new QueryDefinition(sql);

            if (parameters != null)
            {
                PropertyInfo[] infos = parameters.GetType().GetProperties();
                foreach (PropertyInfo info in infos)
                {
                    queryDefinition.WithParameter($"@{info.Name}", info.GetValue(parameters, null));
                }
            }

            return queryDefinition;
        }
    }
}
