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

using System.Globalization;
using System.Text.RegularExpressions;

namespace SmokingDetectorFunctions
{
    class CurrentAlertsFunctions
    {

        [FunctionName("client-get-current-alerts")]
        public static async Task<List<string>> clientGetCurrentAlerts(
   [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "client_get_current_alerts/{PartitionKey}")] HttpRequestMessage request,
   [Table("CurrentAlerts")] CloudTable cloudTable,
   string PartitionKey,
   ILogger log)
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
