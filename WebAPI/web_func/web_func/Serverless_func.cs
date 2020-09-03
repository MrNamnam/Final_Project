using System;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Globalization;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net;
using Newtonsoft.Json.Linq;
using System.IO;
// For hubConnection make sure NuGet install this and not Microsoft.AspNetCore.SignalR.Client.Core
using Microsoft.WindowsAzure.Storage.Table;




/*
namespace signalR_and_web
{
    public static class GenerateClient
    {
        

        public static List<Client> clientslist = new List<Client>();
        static readonly string EndpointUri = "https://cosmosdbtables.documents.azure.com:443/";
        // The primary key for the Azure Cosmos account.
        static readonly string PrimaryKey = "8iBSvw71oPIP96X23WDFNjA08Iv3Xpd4BVd1GL7v9HIqwfX8rLe576xj1aB8znLQWbojs8NI32YhCY7nYhRtLA==";
        static CosmosClient cosmosClient;
        // The database we will create
        static Database database;
        static Microsoft.Azure.Cosmos.Container container;
        // The name of the database and container we will create
        static string databaseId = "Detectors";
        static string containerId = "Events";



        /////////////////////////////////////////////////////// Guy //////////////////////////////////////////////////
        

        // Creating an AzureSignalR object - see AzureSignalR class in this project
        private static readonly AzureSignalR SignalR = new AzureSignalR(Environment.GetEnvironmentVariable("AzureSignalRConnectionString"));

        // This function Deserialize an http requset
        private static async Task<T> ExtractContent<T>(HttpRequestMessage request)
        {
            string connectionRequestJson = await request.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(connectionRequestJson);
        }


        // Initielize the negotiation on signalR
        [FunctionName("negotiate")]
        public static async Task<SignalRConnectionInfo> Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestMessage request,
            ILogger log)
        {
            try
            {
                ConnectionRequest connectionRequest = await ExtractContent<ConnectionRequest>(request);
                log.LogInformation($"Negotiating connection for user: <{connectionRequest.UserId}>.");

                string clientHubUrl = SignalR.GetClientHubUrl("AlertsHub");
                string accessToken = SignalR.GenerateAccessToken(clientHubUrl, connectionRequest.UserId);
                return new SignalRConnectionInfo { AccessToken = accessToken, Url = clientHubUrl };
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to negotiate connection.");
                throw;
            }
        }


        // Internal class SignalRMessage
        public class WebResponseTuple
        {
            // Gets or sets the string contents of the http response
            public string ResponseContents { get; set; }

            // Gets or sets the status code of the http response
            public HttpStatusCode StatusCode { get; set; }

            // Converts the string contents of the http response to a JObject
            // return: JObject of string contents
            public JObject ResponseAsJObject()
            {
                return JObject.Parse(this.ResponseContents);
            }

            // Converts the string contents of the http response to a JArray
            // return: JArray of string contents
            public JArray ResponseAsJArray()
            {
                return JArray.Parse(this.ResponseContents);
            }
        }

        // Utilities for accessing APIs
        public class Utilities
        {
            // Execute the web request for given URL and content and returns response
            // <param name="requestUrl">URL for which to make web request</param>
            // <param name="headers">Authorization headers for this request</param>
            // <param name="content">Content of request</param>
            // <param name="contentType">Content type</param>
            // <param name="method">Method of request</param>
            // <returns>Response from web request</returns>
            public static WebResponseTuple ExecuteWebRequest(string requestUrl, string content = "", string contentType = "application/json", string method = "POST")
            {
                var webRequest = WebRequest.Create(requestUrl);
                webRequest.Method = method;

                if (content.Length > 0)
                {
                    byte[] dataStream = Encoding.UTF8.GetBytes(content);
                    webRequest.ContentType = contentType;
                    // webRequest.Headers.Add(HttpRequestHeader.ContentEncoding.ToString(), Encoding.UTF8.WebName.ToString());
                    // Write request content
                    using (Stream requestStream = webRequest.GetRequestStream())
                    {
                        requestStream.Write(dataStream, 0, dataStream.Length);
                        requestStream.Flush();
                    }
                }

                try
                {
                    // Execute request and get response
                    using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                    {
                        // TODO: Is there a way to get the return code here?
                        using (Stream stream = response.GetResponseStream())
                        {
                            using (StreamReader streamReader = new StreamReader(stream))
                            {
                                var result = streamReader.ReadToEnd();
                                WebResponseTuple tuple = new WebResponseTuple()
                                {
                                    ResponseContents = result,
                                    StatusCode = response.StatusCode
                                };

                                return tuple;
                            }
                        }
                    }
                }
                catch (WebException e)
                {
                    var response = (HttpWebResponse)e.Response;
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader streamReader = new StreamReader(stream))
                        {
                            var result = streamReader.ReadToEnd();
                            WebResponseTuple tuple = new WebResponseTuple()
                            {
                                ResponseContents = result,
                                StatusCode = response.StatusCode
                            };

                            return tuple;
                        }
                    }
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /////////////////////////////////////////////////// web function //////////////////////////////////////////////////////////





        //// http requet for Events container (cosmosDb) to return all events
        ///
        [FunctionName("get-history-events")]
        public static async Task<List<Events>> GetHistoryEvents(
[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "get_history_events")] HttpRequestMessage request,
ILogger log)
        {
            cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
            database = cosmosClient.GetDatabase(databaseId);
            container = database.GetContainer(containerId);
            List<Events> eventsCountArray = new List<Events>() { };
            var sqlQueryText = "SELECT * from c";

            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<Events> queryResultSetIterator = container.GetItemQueryIterator<Events>(queryDefinition);
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Events> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Events i in currentResultSet)
                {
                    Console.WriteLine("\tRead {0}\n", i);
                    eventsCountArray.Add(i);
                }
            }
            return eventsCountArray;
        }



        //// http requet for CurrentAlerts table to return all alerts
        ///
        [FunctionName("get-current-alerts")]
        public static async Task<TableQuerySegment<CurrentAlerts>> GetCurrentAlerts(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "get_current_alerts")] HttpRequestMessage request,
    [Table("CurrentAlerts")] CloudTable cloudTable,
    ILogger log)
        {

            TableQuery<CurrentAlerts> idQuery = new TableQuery<CurrentAlerts>();

            TableQuerySegment<CurrentAlerts> queryResult = await cloudTable.ExecuteQuerySegmentedAsync(idQuery, null);
            
            return queryResult;
        }



        /// we will use the function when we solved any a current alert to delete it from table 
        /// function call DeleteAlert function which will preform the delete itself
        [FunctionName("delete-alert")]
        public static async Task UpdateAlertTable(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "delete-alert/{RowKey}/{PartitionKey}")] HttpRequestMessage request,
            [Table("CurrentAlerts")] CloudTable cloudTable,
            string RowKey,
            string PartitionKey,
            ILogger log)
        {
            await DeleteAlert(cloudTable, RowKey, PartitionKey, log);
            log.LogInformation("delete from table");
        }

        private static async Task DeleteAlert(CloudTable cloudTable, string RowKey, string PartitionKey, ILogger log)
        {
            var filter1 = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, RowKey);
            var filter2 = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, PartitionKey);
            var combinedFilter = TableQuery.CombineFilters(filter1, TableOperators.And, filter2);
            TableQuery<CurrentAlerts> idQuery = new TableQuery<CurrentAlerts>().Where(combinedFilter);

            TableQuerySegment<CurrentAlerts> queryResult = await cloudTable.ExecuteQuerySegmentedAsync(idQuery, null);
            CurrentAlerts cloudRowAlert = queryResult.FirstOrDefault();
            if (cloudRowAlert == null)
            {
                log.LogInformation("There is no such Entity to delete");
            }
            else
            {
                TableOperation deleteOperation = TableOperation.Delete(cloudRowAlert);
                await cloudTable.ExecuteAsync(deleteOperation);
            }
        }



        //// http requet with @parm rowkey - user email in oreder to find it match client data in the client Table
        ///
        [FunctionName("findMatchClientToAlert")]
        public static async Task<Client> findMatchClientToAlert(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "match-alert-to-client/{RowKey}")] HttpRequestMessage request,
            [Table("ClientsTable")] CloudTable cloudTable,
            string RowKey,
            ILogger log)
        {
            return await getClient(cloudTable, RowKey, log);
        }

        private static async Task<Client> getClient(CloudTable cloudTable, string RowKey, ILogger log)
        {
            var filter = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, RowKey);
            TableQuery<Client> idQuery = new TableQuery<Client>().Where(filter);

            TableQuerySegment<Client> queryResult = await cloudTable.ExecuteQuerySegmentedAsync(idQuery, null);
            Client cloudRowAlert = queryResult.FirstOrDefault();
            if (cloudRowAlert == null)
            {
                log.LogInformation("There is no such Client - Error");
                return null;
            }
            else
            {
                return cloudRowAlert;
            }
        }




     

        /// will get the row with max id
        /// use in the function UpdateEventTable in order to add new event after we solved it and delete it from CurrentAlerts table
        private static async Task<int> GetMaxId(Container container)
        {
            var sqlQueryText = "SELECT VALUE MAX(c.event_id) FROM c";

            int count = 0;
            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<int> queryResultSetIterator = container.GetItemQueryIterator<int>(queryDefinition);

            Console.WriteLine("here");

            while (queryResultSetIterator.HasMoreResults)
            {
                Console.WriteLine("here1");
                FeedResponse<int> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (int i in currentResultSet)
                {
                    Console.WriteLine("here2");
                    Console.WriteLine("\tRead {0}\n", i);
                    count = i;
                }
            }
            return count;
        }



        /// will get all event information from the web call 
        /// @post will add event to Events cosmos table
        [FunctionName("add-event")]
        public static async Task UpdateEventTable(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "add-event/{device_id}/{country}/{city}/{email}/{latitude}/{longitude}/{time_str}/{is_false_alarm_str}/{event_details}/{num_of_injured}")] HttpRequestMessage request,
            [SignalR(HubName = "CounterHub")] IAsyncCollector<SignalRMessage> signalRMessages,
            string device_id, string country, string city, string email, string latitude, string longitude, string time_str,
            string is_false_alarm_str, string event_details, string num_of_injured,
            ILogger log)
        {
            cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
            database = cosmosClient.GetDatabase(databaseId);
            container = database.GetContainer(containerId);
            int event_id = await GetMaxId(container) + 1;
            Events newEvent = new Events
            {
                id = event_id.ToString(),
                event_id = event_id,
                device_id = device_id,
                email = email,
                country = country,
                city = city,
                latitude = latitude,
                longitude = longitude,
                is_false_alarm = is_false_alarm_str,
                event_details = event_details,
                num_of_injured = num_of_injured,
                time = time_str
            };
            log.LogInformation(newEvent.id.ToString());
            try
            {
                // Read the item to see if it exists.  
                ItemResponse<Events> newEventResponse = await container.CreateItemAsync<Events>(newEvent, new PartitionKey(newEvent.email));
                log.LogInformation("Item with id: {0} and email {1} was added\n", newEventResponse.Resource.id, newEventResponse.Resource.email);

                
            }
            catch (CosmosException ex)
            {

                log.LogInformation("Error adding item to events table: {0}\n", ex);
            }

        }


        /// the function will return a list of EventdayCount which will be use to plot the number of event each day graph
        [FunctionName("agg-event-day")]
        public static async Task<List<EventdayCount>> AggEventsEachDay(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "agg-event-day")] HttpRequestMessage request,
            [SignalR(HubName = "CounterHub")] IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log)
        {
            cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
            database = cosmosClient.GetDatabase(databaseId);
            container = database.GetContainer(containerId);
            List<EventdayCount> eventsCountArray = new List<EventdayCount>() { };
            var sqlQueryText = "SELECT table.time as x, count(table.event_id) as y from" +
                " (SELECT c.event_id as event_id, SUBSTRING(c.time, 0, 10) as time  FROM c) as table" +
                " GROUP BY table.time";

            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<EventdayCount> queryResultSetIterator = container.GetItemQueryIterator<EventdayCount>(queryDefinition);
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<EventdayCount> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (EventdayCount i in currentResultSet)
                {
                    Console.WriteLine("\tRead {0}\n", i);
                    eventsCountArray.Add(i);
                }
            }
            eventsCountArray.Sort((x, y) => DateTime.Compare(x.x, y.x));
            return eventsCountArray;


        }


        ///the function will return a list of NumOfInjuredCount which will be use to plot how many events had each number of injurd people
        [FunctionName("agg-city-count")]
        public static async Task<List<CityCount>> AggCityCount(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "agg-city-count")] HttpRequestMessage request,
    ILogger log)
        {
            cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
            database = cosmosClient.GetDatabase(databaseId);
            container = database.GetContainer(containerId);
            List<CityCount> eventsCountArray = new List<CityCount>() { };
            var sqlQueryText = "SELECT c.city as name, count(c.event_id) as y from c " +
                "GROUP BY c.city";


            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<CityCount> queryResultSetIterator = container.GetItemQueryIterator<CityCount>(queryDefinition);
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<CityCount> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (CityCount i in currentResultSet)
                {
                    Console.WriteLine("\tRead {0}\n", i);
                    eventsCountArray.Add(i);
                }
            }
            return eventsCountArray;


        }


        /// still need to complete the function -------------------------------------------------!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        /// the function will return a list of NumOfEventsPerCity which will be use to plot how many events (true and false) happened in each city
        [FunctionName("agg-injured-count")]
        public static async Task<List<NumOfInjuredCount>> AggInjuredCount(
[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "agg-injured--count")] HttpRequestMessage request,
[SignalR(HubName = "CounterHub")] IAsyncCollector<SignalRMessage> signalRMessages,
ILogger log)
        {
            cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
            database = cosmosClient.GetDatabase(databaseId);
            container = database.GetContainer(containerId);
            List<NumOfInjuredCount> eventsCountArray = new List<NumOfInjuredCount>() { };
            var sqlQueryText = "SELECT c.num_of_injured as label, count(c.event_id) as y from c " +
                "WHERE c.is_false_alarm = 'False' " +
                "GROUP BY c.num_of_injured"; 

            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<NumOfInjuredCount> queryResultSetIterator = container.GetItemQueryIterator<NumOfInjuredCount>(queryDefinition);
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<NumOfInjuredCount> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (NumOfInjuredCount i in currentResultSet)
                {
                    Console.WriteLine("\tRead {0}\n", i);
                    eventsCountArray.Add(i);
                }
            }
            return eventsCountArray;


        }


        //// http call for sending a chat message from web to client
        /// @param: client email as a signal R target
        /// send signal r to client device
        [FunctionName("WebSendMessgae")]
        public static async Task webSendMessgae(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "web-send-message/{user}/{message}")] HttpRequestMessage request,
    [SignalR(HubName = "AlertsHub")] IAsyncCollector<SignalRMessage> signalRMessages,
    string user,
    string message,
    ILogger log)
        {

            await signalRMessages.AddAsync(
                 new SignalRMessage
                 {
                     UserId = user,
                     Target = "Chat",
                     Arguments = new object[] { message }
                 }
             );
            log.LogInformation("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! sent");
        }





        //// http call for sending a chat message from client to web
        /// @param: client email as a signal R target and message
        /// send signal r to client device
        [FunctionName("ClientDeleteMessgae")]
        public static async Task clientDeleteMessage(
   [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "client-delete-message/{user}/{deviceid}")] HttpRequestMessage request,
   [SignalR(HubName = "AlertsHub")] IAsyncCollector<SignalRMessage> signalRMessages,
   string user,
   string deviceid,
   ILogger log)
        {

            await signalRMessages.AddAsync(
                 new SignalRMessage
                 {
                     UserId = user,
                     Target = "DoneWithAlert",
                     Arguments = new object[] { deviceid }
                 }
             );
            log.LogInformation("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! delete message sent");
        }

    }
}


*/

namespace signalR_and_web
{
    public static class GenerateClient
    {


        public static List<Client> clientslist = new List<Client>();
        static readonly string EndpointUri = "https://cosmosdbtables.documents.azure.com:443/";
        // The primary key for the Azure Cosmos account.
        static readonly string PrimaryKey = "8iBSvw71oPIP96X23WDFNjA08Iv3Xpd4BVd1GL7v9HIqwfX8rLe576xj1aB8znLQWbojs8NI32YhCY7nYhRtLA==";
        static CosmosClient cosmosClient;
        // The database we will create
        static Database database;
        static Microsoft.Azure.Cosmos.Container container;
        // The name of the database and container we will create
        static string databaseId = "Detectors";
        static string containerId = "Events";



        /////////////////////////////////////////////////////// Guy //////////////////////////////////////////////////


        // Creating an AzureSignalR object - see AzureSignalR class in this project
        private static readonly AzureSignalR SignalR = new AzureSignalR(Environment.GetEnvironmentVariable("AzureSignalRConnectionString"));

        // This function Deserialize an http requset
        private static async Task<T> ExtractContent<T>(HttpRequestMessage request)
        {
            string connectionRequestJson = await request.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(connectionRequestJson);
        }


        // Initielize the negotiation on signalR
        [FunctionName("negotiate")]
        public static async Task<SignalRConnectionInfo> Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestMessage request,
            ILogger log)
        {
            try
            {
                ConnectionRequest connectionRequest = await ExtractContent<ConnectionRequest>(request);
                log.LogInformation($"Negotiating connection for user: <{connectionRequest.UserId}>.");

                string clientHubUrl = SignalR.GetClientHubUrl("AlertsHub");
                string accessToken = SignalR.GenerateAccessToken(clientHubUrl, connectionRequest.UserId);
                return new SignalRConnectionInfo { AccessToken = accessToken, Url = clientHubUrl };
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to negotiate connection.");
                throw;
            }
        }


        // Internal class SignalRMessage
        public class WebResponseTuple
        {
            // Gets or sets the string contents of the http response
            public string ResponseContents { get; set; }

            // Gets or sets the status code of the http response
            public HttpStatusCode StatusCode { get; set; }

            // Converts the string contents of the http response to a JObject
            // return: JObject of string contents
            public JObject ResponseAsJObject()
            {
                return JObject.Parse(this.ResponseContents);
            }

            // Converts the string contents of the http response to a JArray
            // return: JArray of string contents
            public JArray ResponseAsJArray()
            {
                return JArray.Parse(this.ResponseContents);
            }
        }

        // Utilities for accessing APIs
        public class Utilities
        {
            // Execute the web request for given URL and content and returns response
            // <param name="requestUrl">URL for which to make web request</param>
            // <param name="headers">Authorization headers for this request</param>
            // <param name="content">Content of request</param>
            // <param name="contentType">Content type</param>
            // <param name="method">Method of request</param>
            // <returns>Response from web request</returns>
            public static WebResponseTuple ExecuteWebRequest(string requestUrl, string content = "", string contentType = "application/json", string method = "POST")
            {
                var webRequest = WebRequest.Create(requestUrl);
                webRequest.Method = method;

                if (content.Length > 0)
                {
                    byte[] dataStream = Encoding.UTF8.GetBytes(content);
                    webRequest.ContentType = contentType;
                    // webRequest.Headers.Add(HttpRequestHeader.ContentEncoding.ToString(), Encoding.UTF8.WebName.ToString());
                    // Write request content
                    using (Stream requestStream = webRequest.GetRequestStream())
                    {
                        requestStream.Write(dataStream, 0, dataStream.Length);
                        requestStream.Flush();
                    }
                }

                try
                {
                    // Execute request and get response
                    using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                    {
                        // TODO: Is there a way to get the return code here?
                        using (Stream stream = response.GetResponseStream())
                        {
                            using (StreamReader streamReader = new StreamReader(stream))
                            {
                                var result = streamReader.ReadToEnd();
                                WebResponseTuple tuple = new WebResponseTuple()
                                {
                                    ResponseContents = result,
                                    StatusCode = response.StatusCode
                                };

                                return tuple;
                            }
                        }
                    }
                }
                catch (WebException e)
                {
                    var response = (HttpWebResponse)e.Response;
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader streamReader = new StreamReader(stream))
                        {
                            var result = streamReader.ReadToEnd();
                            WebResponseTuple tuple = new WebResponseTuple()
                            {
                                ResponseContents = result,
                                StatusCode = response.StatusCode
                            };

                            return tuple;
                        }
                    }
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /////////////////////////////////////////////////// web function //////////////////////////////////////////////////////////





        //// http requet for Events container (cosmosDb) to return all events
        ///
        [FunctionName("get-history-events")]
        public static async Task<List<Events>> GetHistoryEvents(
[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "get_history_events")] HttpRequestMessage request,
ILogger log)
        {
            cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
            database = cosmosClient.GetDatabase(databaseId);
            container = database.GetContainer(containerId);
            List<Events> eventsCountArray = new List<Events>() { };
            var sqlQueryText = "SELECT * from c";

            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<Events> queryResultSetIterator = container.GetItemQueryIterator<Events>(queryDefinition);
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Events> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Events i in currentResultSet)
                {
                    Console.WriteLine("\tRead {0}\n", i);
                    eventsCountArray.Add(i);
                }
            }
            return eventsCountArray;
        }





        /// will get the row with max id
        /// use in the function UpdateEventTable in order to add new event after we solved it and delete it from CurrentAlerts table
        private static async Task<int> GetMaxId(Container container)
        {
            var sqlQueryText = "SELECT VALUE MAX(c.event_id) FROM c";

            int count = 0;
            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<int> queryResultSetIterator = container.GetItemQueryIterator<int>(queryDefinition);

            Console.WriteLine("here");

            while (queryResultSetIterator.HasMoreResults)
            {
                Console.WriteLine("here1");
                FeedResponse<int> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (int i in currentResultSet)
                {
                    Console.WriteLine("here2");
                    Console.WriteLine("\tRead {0}\n", i);
                    count = i;
                }
            }
            return count;
        }



        /// will get all event information from the web call 
        /// @post will add event to Events cosmos table
        [FunctionName("add-event")]
        public static async Task UpdateEventTable(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "add-event/{device_id}/{country}/{city}/{email}/{latitude}/{longitude}/{time_str}/{is_false_alarm_str}/{event_details}/{num_of_injured}")] HttpRequestMessage request,
            [SignalR(HubName = "CounterHub")] IAsyncCollector<SignalRMessage> signalRMessages,
            string device_id, string country, string city, string email, string latitude, string longitude, string time_str,
            string is_false_alarm_str, string event_details, string num_of_injured,
            ILogger log)
        {
            cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
            database = cosmosClient.GetDatabase(databaseId);
            container = database.GetContainer(containerId);
            int event_id = await GetMaxId(container) + 1;
            Events newEvent = new Events
            {
                id = event_id.ToString(),
                event_id = event_id,
                device_id = device_id,
                email = email,
                country = country,
                city = city,
                latitude = latitude,
                longitude = longitude,
                is_false_alarm = is_false_alarm_str,
                event_details = event_details,
                num_of_injured = num_of_injured,
                time = time_str
            };
            log.LogInformation(newEvent.id.ToString());
            try
            {
                // Read the item to see if it exists.  
                ItemResponse<Events> newEventResponse = await container.CreateItemAsync<Events>(newEvent, new PartitionKey(newEvent.email));
                log.LogInformation("Item with id: {0} and email {1} was added\n", newEventResponse.Resource.id, newEventResponse.Resource.email);


            }
            catch (CosmosException ex)
            {

                log.LogInformation("Error adding item to events table: {0}\n", ex);
            }

        }


        /// the function will return a list of EventdayCount which will be use to plot the number of event each day graph
        [FunctionName("agg-event-day")]
        public static async Task<List<EventdayCount>> AggEventsEachDay(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "agg-event-day")] HttpRequestMessage request,
            [SignalR(HubName = "CounterHub")] IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log)
        {
            cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
            database = cosmosClient.GetDatabase(databaseId);
            container = database.GetContainer(containerId);
            List<EventdayCount> eventsCountArray = new List<EventdayCount>() { };
            var sqlQueryText = "SELECT table.time as x, count(table.event_id) as y from" +
                " (SELECT c.event_id as event_id, SUBSTRING(c.time, 0, 10) as time  FROM c) as table" +
                " GROUP BY table.time";

            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<EventdayCount> queryResultSetIterator = container.GetItemQueryIterator<EventdayCount>(queryDefinition);
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<EventdayCount> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (EventdayCount i in currentResultSet)
                {
                    Console.WriteLine("\tRead {0}\n", i);
                    eventsCountArray.Add(i);
                }
            }
            eventsCountArray.Sort((x, y) => DateTime.Compare(x.x, y.x));
            return eventsCountArray;


        }


        ///the function will return a list of NumOfInjuredCount which will be use to plot how many events had each number of injurd people
        [FunctionName("agg-city-count")]
        public static async Task<List<CityCount>> AggCityCount(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "agg-city-count")] HttpRequestMessage request,
    ILogger log)
        {
            cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
            database = cosmosClient.GetDatabase(databaseId);
            container = database.GetContainer(containerId);
            List<CityCount> eventsCountArray = new List<CityCount>() { };
            var sqlQueryText = "SELECT c.city as name, count(c.event_id) as y from c " +
                "GROUP BY c.city";


            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<CityCount> queryResultSetIterator = container.GetItemQueryIterator<CityCount>(queryDefinition);
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<CityCount> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (CityCount i in currentResultSet)
                {
                    Console.WriteLine("\tRead {0}\n", i);
                    eventsCountArray.Add(i);
                }
            }
            return eventsCountArray;


        }


        /// still need to complete the function -------------------------------------------------!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        /// the function will return a list of NumOfEventsPerCity which will be use to plot how many events (true and false) happened in each city
        [FunctionName("agg-injured-count")]
        public static async Task<List<NumOfInjuredCount>> AggInjuredCount(
[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "agg-injured--count")] HttpRequestMessage request,
[SignalR(HubName = "CounterHub")] IAsyncCollector<SignalRMessage> signalRMessages,
ILogger log)
        {
            cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
            database = cosmosClient.GetDatabase(databaseId);
            container = database.GetContainer(containerId);
            List<NumOfInjuredCount> eventsCountArray = new List<NumOfInjuredCount>() { };
            var sqlQueryText = "SELECT c.num_of_injured as label, count(c.event_id) as y from c " +
                "WHERE c.is_false_alarm = 'False' " +
                "GROUP BY c.num_of_injured";

            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<NumOfInjuredCount> queryResultSetIterator = container.GetItemQueryIterator<NumOfInjuredCount>(queryDefinition);
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<NumOfInjuredCount> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (NumOfInjuredCount i in currentResultSet)
                {
                    Console.WriteLine("\tRead {0}\n", i);
                    eventsCountArray.Add(i);
                }
            }
            return eventsCountArray;


        }


        //// http call for sending a chat message from web to client
        /// @param: client email as a signal R target
        /// send signal r to client device
        [FunctionName("WebSendMessgae")]
        public static async Task webSendMessgae(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "web-send-message/{user}/{message}")] HttpRequestMessage request,
    [SignalR(HubName = "AlertsHub")] IAsyncCollector<SignalRMessage> signalRMessages,
    string user,
    string message,
    ILogger log)
        {

            await signalRMessages.AddAsync(
                 new SignalRMessage
                 {
                     UserId = user,
                     Target = "Chat",
                     Arguments = new object[] { message }
                 }
             );
            log.LogInformation("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! sent");
        }





        //// http call for sending a chat message from client to web
        /// @param: client email as a signal R target and message
        /// send signal r to client device
        [FunctionName("ClientDeleteMessgae")]
        public static async Task clientDeleteMessage(
   [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "client-delete-message/{user}/{deviceid}")] HttpRequestMessage request,
   [SignalR(HubName = "AlertsHub")] IAsyncCollector<SignalRMessage> signalRMessages,
   string user,
   string deviceid,
   ILogger log)
        {

            await signalRMessages.AddAsync(
                 new SignalRMessage
                 {
                     UserId = user,
                     Target = "DoneWithAlert",
                     Arguments = new object[] { deviceid }
                 }
             );
            log.LogInformation("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! delete message sent");
        }

    }
}




