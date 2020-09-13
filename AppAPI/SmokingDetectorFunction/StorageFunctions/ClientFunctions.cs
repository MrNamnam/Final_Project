using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace SmokingDetectorFunctions
{
    // Summary: this class contains all the functions related to the clients themselfs
    public static class ClientFunctions
    {
        // Summary: when first entring the app, a client register with all his personal information
        //          to help the firemen contact the person quickly
        // Params: email
        //         name
        //         password
        //         phone_number
        // Return: if all fields are fine, return OkObjectResult and add the user to DB
        //         otherwise, return BadRequestObjectResult with the relevant message

        [FunctionName("Register")]
        public static async Task<IActionResult> Register(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "Register/{email}/{password}/{name}/{phone_number}/{favorite_color}")] HttpRequestMessage request,
           [Table("ClientsTable")] CloudTable table,
           [SignalR(HubName = "CounterHub")] IAsyncCollector<SignalRMessage> signalRMessages,
           string email, string password, string name, string phone_number, string favorite_color,
           ILogger log)
        {
            log.LogInformation("Registering a new user!\n");

            ////////////////// Checking that the email address is not already registered ////////
            var query = new TableQuery<DynamicTableEntity>()
            {
                SelectColumns = new List<string>()
                {
                    "PartitionKey"
                }
            };
            var queryOutput = table.ExecuteQuerySegmentedAsync<DynamicTableEntity>(query, null);
            var results = queryOutput.Result;
            foreach (var entity in results)
            {
                if (email == entity.PartitionKey)
                {
                    return new BadRequestObjectResult("Email address already exist.\n" +
                        "Please register with other email or sign in with this email");
                }
            }

            /////////////////// Inserting the new device to the table //////////////////
            Client new_client = new Client(email, password, name, phone_number, favorite_color);

            TableOperation insert = TableOperation.Insert(new_client);
            await table.ExecuteAsync(insert);
            return (ActionResult)new OkObjectResult($"Thanks {name}, you have registered successfully with: {email}");
        }

        // Summary: Called when signing in to the app
        // Params:  email (the email addres of the client)
        //          password (the password that the client hed entered)
        // Reutrn:  IActionResult - OK if everiting fine
        //                          BAD if email does not exist or incorrct password
        [FunctionName("SignIn")]
        public static async Task<IActionResult> SignIn(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "SignIn/{email}/{password}")] HttpRequestMessage request,
           [Table("ClientsTable")] CloudTable ClientsTable,
           [SignalR(HubName = "CounterHub")] IAsyncCollector<SignalRMessage> signalRMessages,
           string email, string password,
           ILogger log)
        {
            log.LogInformation("Signing in!\n");

            string emailFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, email);

            var emailQuery = new TableQuery<Client>().Where(emailFilter);
            TableQuerySegment<Client> queryResult = await ClientsTable.ExecuteQuerySegmentedAsync(emailQuery, null);
            Client client = queryResult.FirstOrDefault();
            if (client == null)
            {
                return new BadRequestObjectResult("Email does not exist.\n" +
                        "Please register before signing in");
            }
            else if (client.password != password)
            {
                return new BadRequestObjectResult("Incorrect password.");
            }
            else
            {
                return (ActionResult)new OkObjectResult($"{client.name} signed in successfully");
            }
        }

        // Summary: Listing all the devices of client with email (@param: email)
        // Params:  email (the email addres of the client)
        // Reutrn:  All the devices this client have
        //          as List<SmokingDetector>
        [FunctionName("ListDevices")]
        public static async Task<List<SmokingDetector>> ListDevices(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "ListDevices/{email}")] HttpRequestMessage request,
           [Table("DetectorsEntities")] CloudTable DetectorsEntities,
           [SignalR(HubName = "CounterHub")] IAsyncCollector<SignalRMessage> signalRMessages,
           string email,
           ILogger log)
        {
            log.LogInformation("Listing devices\n");

            var devices = new List<SmokingDetector>();

            string emailFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, email);
            var emailQuery = new TableQuery<SmokingDetector>().Where(emailFilter);

            TableContinuationToken continuationToken = null;
            do
            {
                var newEntity = await DetectorsEntities.ExecuteQuerySegmentedAsync(emailQuery, continuationToken);
                continuationToken = newEntity.ContinuationToken;
                devices.AddRange(newEntity.Results);
            }
            while (continuationToken != null);

            return devices;
        }

        [FunctionName("CheckColor")]
        public static async Task<IActionResult> CheckColor(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "CheckColor/{email}/{favorite_color}")] HttpRequestMessage request,
        [Table("ClientsTable")] CloudTable ClientsTable,
        string email, string favorite_color,
        ILogger log)
        {
            log.LogInformation("ForgotPassword!\n");

            string emailFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, email);

            var emailQuery = new TableQuery<Client>().Where(emailFilter);
            TableQuerySegment<Client> queryResult = await ClientsTable.ExecuteQuerySegmentedAsync(emailQuery, null);
            Client client = queryResult.FirstOrDefault();
            if (client == null)
            {
                return new BadRequestObjectResult("Email does not exist.\n" +
                        "Please register before");
            }
            if ( string.Compare(favorite_color, client.favorite_color) != 0)
            {
                return new BadRequestObjectResult("Oops, The color does not match.");
            }
            return (ActionResult)new OkObjectResult("OK");
        }

        [FunctionName("UpdatePassword")]
        public static async Task<IActionResult> UpdatePassword(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "UpdatePassword/{email}/{new_password}")] HttpRequestMessage request,
        [Table("ClientsTable")] CloudTable ClientsTable,
        string email, string new_password,
        ILogger log)
        {
            log.LogInformation("Uodating password!\n");

            TableOperation retrieve = TableOperation.Retrieve<Client>(email, email);
            TableResult result = await ClientsTable.ExecuteAsync(retrieve);

            Client client = (Client)result.Result;

            client.password = new_password;

            if (result != null)
            {
                TableOperation update = TableOperation.Replace(client);

                await ClientsTable.ExecuteAsync(update);
                return (ActionResult)new OkObjectResult("OK");
            }
            return new BadRequestObjectResult("Something went wrong.");
        }
    }
}


