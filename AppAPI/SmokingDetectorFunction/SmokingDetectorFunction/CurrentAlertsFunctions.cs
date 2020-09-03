using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Net.Http;
using System.Collections.Generic;


namespace SmokingDetectorFunction
{
    public static class CurrentAlertsFunctions
    {

        [FunctionName("client-get-current-alerts")]
        public static async Task<List<string>> clientGetCurrentAlerts(
   [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "client_get_current_alerts/{PartitionKey}")] HttpRequestMessage request,
    [Table("CurrentAlerts")] CloudTable cloudTable, string PartitionKey
   )
        {
            List<string> deviceid_array = new List<string> { };
            var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, PartitionKey);

            TableQuery<CurrentAlerts> idQuery = new TableQuery<CurrentAlerts>().Where(filter);

            TableQuerySegment<CurrentAlerts> queryResult = await cloudTable.ExecuteQuerySegmentedAsync(idQuery, null);
            foreach (CurrentAlerts alert in queryResult)
            {
                deviceid_array.Add(alert.RowKey);
            }
            return deviceid_array;
        }

    }
}
