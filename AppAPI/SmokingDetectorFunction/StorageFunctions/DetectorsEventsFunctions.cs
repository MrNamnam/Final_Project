using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System.Net.Http;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos;
using System;

namespace SmokingDetectorFunctions
{
    public static class DetectorEventsFunctions
    {
        // The Azure Cosmos DB endpoint for running this sample.
        static readonly string EndpointUri = "https://cosmosdbtables.documents.azure.com:443/";
        // The primary key for the Azure Cosmos account.
        static readonly string PrimaryKey = "8iBSvw71oPIP96X23WDFNjA08Iv3Xpd4BVd1GL7v9HIqwfX8rLe576xj1aB8znLQWbojs8NI32YhCY7nYhRtLA==";
        static CosmosClient cosmosClient;
        // The database we will create
        static Database database;
        static Container container;
        // The name of the database and container we will create
        static string databaseId = "Detectors";
        static string containerId = "Events";

        [FunctionName("ListEvents")]
        public static async Task<List<Events>> ListDevices(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "ListEvents/{email}/{device_name}")] HttpRequestMessage request,
           [Table("DetectorsEntities")] CloudTable DetectorsEntities,
           string email, string device_name,
           ILogger log)
        {
            cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
            database = cosmosClient.GetDatabase(databaseId);
            container = database.GetContainer(containerId);
            log.LogInformation("Listing events\n");

            ////////////// Finding the device id ////////////////
            string device_id = null;

            string emailFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, email);
            var emailQuery = new TableQuery<SmokingDetector>().Where(emailFilter);

            TableContinuationToken continuationToken = null;
            do
            {
                var newEntity = await DetectorsEntities.ExecuteQuerySegmentedAsync(emailQuery, continuationToken);
                continuationToken = newEntity.ContinuationToken;
                foreach (SmokingDetector device in newEntity.Results)
                {
                    if (device.device_name == device_name)
                    {
                        device_id = device.RowKey;
                    }
                }

            }
            while (continuationToken != null);

            ///////////// Listing all events ///////
            return await GetEventsByEmailAndDeviceId(container, device_id, email);
        }



        public static async Task<List<Events>> GetEventsByEmailAndDeviceId(Container container, string device_id, string email)
        {
            var sqlQueryText = "SELECT * FROM c WHERE c.device_id = '" + device_id + "' AND c.email = '" + email + "' ORDER BY c.event_id";

            List<Events> eventsList = new List<Events>();
            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<Events> queryResultSetIterator = container.GetItemQueryIterator<Events>(queryDefinition);

            Console.WriteLine("here");

            while (queryResultSetIterator.HasMoreResults)
            {
                Console.WriteLine("here1");
                FeedResponse<Events> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Events Event in currentResultSet)
                {
                    Console.WriteLine("\tRead {0}\n", Event);
                    eventsList.Add(Event);
                }
            }
            return eventsList;
        }

    }
}