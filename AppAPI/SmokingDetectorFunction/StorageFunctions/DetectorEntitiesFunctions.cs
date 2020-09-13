using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace SmokingDetectorFunctions
{
    public static class DetectorEntitiesFunctions
    {
        [FunctionName("AddDevice")]
        public static async Task<IActionResult> AddDevice(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "AddDevice/{email}/{device_name}/{address}/{version}/{longitude}/{latitude}")] HttpRequestMessage request,
           [Table("DetectorsEntities")] CloudTable table,
           [SignalR(HubName = "CounterHub")] IAsyncCollector<SignalRMessage> signalRMessages,
           string email, string device_name, string address, string version, string longitude, string latitude,
           ILogger log)
        { 
            log.LogInformation("Adding device.\n");

            /////////////// Checking that device_name is unique ////////
            string emailFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, email);
            var emailQuery = new TableQuery<SmokingDetector>().Where(emailFilter);

            TableContinuationToken continuationToken = null;
            do
            {
                var newEntity = await table.ExecuteQuerySegmentedAsync(emailQuery, continuationToken);
                continuationToken = newEntity.ContinuationToken;
                foreach (SmokingDetector device in newEntity.Results)
                {
                    if(device.device_name == device_name)
                    {
                        return new BadRequestObjectResult("Device name already taken, sorry...");
                    }
                }

            }
            while (continuationToken != null);

            /////////////// Finding device_id /////////////
            long max_id = 0;

            var query = new TableQuery<DynamicTableEntity>()
            {
                SelectColumns = new List<string>()
                {
                    "RowKey"
                }
            };

            var queryOutput = table.ExecuteQuerySegmentedAsync<DynamicTableEntity>(query, null);
            var results = queryOutput.Result;
            foreach (var entity in results)
            {
                if (max_id <= long.Parse(entity.RowKey))
                {
                    max_id = long.Parse(entity.RowKey);
                }
            }

            /////////////////// Inserting the new device to the table //////////////////
            SmokingDetector new_device = new SmokingDetector(email, (max_id+1).ToString(), device_name,
                                                             address, version, longitude, latitude);

            TableOperation insert = TableOperation.Insert(new_device);
            await table.ExecuteAsync(insert);
            return (ActionResult)new OkObjectResult($"Added device: {device_name}");
        }


        [FunctionName("GetDevice")]
        public static async Task<SmokingDetector> GetDevice(
         [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "GetDevice/{email}/{id}")] HttpRequestMessage request,
         [Table("DetectorsEntities")] CloudTable ClientsTable,
         string email, string id,
         ILogger log)
        {
            log.LogInformation("GetDevice!\n");

            string emailFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, email);
            string idFilter = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id);
            var combinedFilter = TableQuery.CombineFilters(emailFilter, TableOperators.And, idFilter);

            var Query = new TableQuery<SmokingDetector>().Where(combinedFilter);
            TableQuerySegment<SmokingDetector> queryResult = await ClientsTable.ExecuteQuerySegmentedAsync(Query, null);
            SmokingDetector detector = queryResult.FirstOrDefault();
            if (detector == null)
            {
                return null;
            }
            return detector;
        }

        [FunctionName("FalseAlarm")]
        public static async Task<IActionResult> FalseAlarm(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "FalseAlarm/{email}/{id}")] HttpRequestMessage request,
        [SignalR(HubName = "AlertsHub")] IAsyncCollector<SignalRMessage> signalRMessages,
        string email, string id,
        ILogger log)
        {
            log.LogInformation("FalseAlarm!\n");

            await signalRMessages.AddAsync(
                    new SignalRMessage
                    {
                        UserId = "FireWeb",
                        Target = "FalseAlarm",
                        Arguments = new[] { $"{email}/{id}" }
                    });

            return (ActionResult)new OkObjectResult($"Done!");
        }

        [FunctionName("chat")]
        public static async Task<ActionResult> chat(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "chat/{message}/{email}")] HttpRequestMessage request,
        [SignalR(HubName = "AlertsHub")] IAsyncCollector<SignalRMessage> signalRMessages,
        string message,
        string email,
        ILogger log)
        {
            log.LogInformation("chat!\n");

            await signalRMessages.AddAsync(
                    new SignalRMessage
                    {
                        UserId = "FireWebChat",
                        Target = email,
                        Arguments = new[] { $"{message}" }
                    });
            return (ActionResult)new OkObjectResult($"Done!");
        }

        [FunctionName("DeleteDevice")]
        public static async Task<IActionResult> DeleteDevice(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "DeleteDevice/{email}/{device_name}")] HttpRequestMessage request,
           [Table("DetectorsEntities")] CloudTable table,
           [SignalR(HubName = "CounterHub")] IAsyncCollector<SignalRMessage> signalRMessages,
           string email, string device_name,
           ILogger log)
        {
            log.LogInformation("Deleting device.\n");

            string device_id = null;

            string emailFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, email);
            var emailQuery = new TableQuery<SmokingDetector>().Where(emailFilter);

            TableContinuationToken continuationToken = null;
            do
            {
                var newEntity = await table.ExecuteQuerySegmentedAsync(emailQuery, continuationToken);
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

            if (device_id == null)
            {
                return new BadRequestObjectResult($"No device: {device_name} for email: {email}");
            }

            TableOperation retrieve = TableOperation.Retrieve<SmokingDetector>(email, device_id);
            TableResult result = await table.ExecuteAsync(retrieve);
            var deleteEntity = (SmokingDetector)result.Result;

            TableOperation delete = TableOperation.Delete(deleteEntity);
            await table.ExecuteAsync(delete);

            return (ActionResult)new OkObjectResult($"Deleted device: {device_name}");
        }
    }
}

