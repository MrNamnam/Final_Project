using System;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Globalization;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System.Drawing;

namespace Eventtrigger
{
    public static class GenerateClient
    {
        public static List<Client> clientslist = new List<Client>();
        public static List<ClientDetectorsObj> clientsdetectors = new List<ClientDetectorsObj>();
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

        public static List<string> Colors = new List<string>(){ "blue", "green", "black", "white", "green", "yellow", "brown", "red", "orange", "purple", "pink", "grey", "azure" };




        [FunctionName("MessageReceiver")]
        public static async Task Run([EventHubTrigger("evenhubdevice", Connection = "EventHub")] System.Collections.Generic.Dictionary<string, string> events,
        [SignalR(HubName = "AlertsHub")]
        IAsyncCollector<SignalRMessage> signalRMessages, 
        ILogger log)
        {
            log.LogInformation("C# Event Iot trigger function processed a request.");

            string accountName = "smokingdetectors";
            string accountKey = "J7cW0Kw5zn6grdBqlw7ZgFxSJU6WZbYyOq2/AJH3qktLCBBSpPBQGazGQ0TmZR27p6YT9RkkTDFl4Ok2Yo0kfQ==";

            StorageCredentials credentials = new StorageCredentials(accountName, accountKey);
            CloudStorageAccount storageAccount = new CloudStorageAccount(credentials, true);

            log.LogInformation(events["email"] + events["device_id"] + events["longitude"] + events["latitude"]);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference("CurrentAlerts");

            DateTime time = DateTime.Now;

            if(events["action"] == "add_one_event")
            {
                CurrentAlerts CurrentAlertsRow = new CurrentAlerts(events["email"], events["device_id"], events["longitude"], events["latitude"], time);

                TableOperation updateAlert = TableOperation.InsertOrReplace(CurrentAlertsRow);

                await table.ExecuteAsync(updateAlert);

                // SignalR part

                await signalRMessages.AddAsync(
                     new SignalRMessage
                     {
                         UserId = events["email"].ToString(),
                         Target = "NewAlertApp",
                         Arguments = new object[] { CurrentAlertsRow }
                     }
                 );
                log.LogInformation("sent signalR" + events["email"]);
            }
            else
            {
                await Simulation(tableClient);
            }

            

        }

        public static async Task Simulation(CloudTableClient tableClient)
        {
            List<SmokingDetector> detectorsArray = new List<SmokingDetector>();
            CloudTable clientsTable = tableClient.GetTableReference("ClientsTable");
            CloudTable detectorsTable = tableClient.GetTableReference("DetectorsEntities");
            cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
            database = cosmosClient.GetDatabase(databaseId);
            container = database.GetContainer(containerId);
            Random r = new Random();
            int event_id = 0;

            for(int i = 0; i <= 50; i++)
            {
                Client newclient = CreateClient(i);
                Console.WriteLine("Finished to create client to array");
                clientslist.Add(newclient);
                try
                {
                    TableOperation updateClient = TableOperation.InsertOrReplace(newclient);
                    Console.WriteLine("Here1, {0}, {1}, {2}, {3}, {4}, {5}", newclient.RowKey, newclient.PartitionKey, newclient.password, newclient.phone_number, newclient.name, newclient.time);
                    await clientsTable.ExecuteAsync(updateClient);
                    Console.WriteLine("Success Inserting client: {0}", newclient.PartitionKey);
                }
                catch
                {
                    Console.WriteLine("Failed Inserting client: {0} @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@", newclient.PartitionKey);
                }

            }
            Console.WriteLine("Here {0}", clientslist[0].PartitionKey);
            foreach (Client client in clientslist)
            {
                Console.WriteLine("Here1");
                detectorsArray = new List<SmokingDetector>();
                r.Next(1, 4);
                int NumOfDetectors = r.Next(1, 4);
                int detector_id = 1;
                Console.WriteLine("Here2");
                for (int i = 0; i <= NumOfDetectors; i++)
                {
                    SmokingDetector newdetector = CreateDetectors(detector_id, client.PartitionKey);
                    Console.WriteLine("Finished to create detctor");
                    detectorsArray.Add(newdetector);
                    try
                    {
                        TableOperation updateAlert = TableOperation.InsertOrReplace(newdetector);

                        await detectorsTable.ExecuteAsync(updateAlert);
                        Console.WriteLine("Success Inserting client: {0}", newdetector.PartitionKey);
                    }
                    catch
                    {
                        Console.WriteLine("Failed Inserting client: {0} @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@", newdetector.PartitionKey);
                    }

                    await CreateEvent(event_id, detector_id.ToString(), newdetector.PartitionKey, newdetector.latitude, newdetector.longitude, container);
                    detector_id++;
                    event_id++;
                }
                clientsdetectors.Add(new ClientDetectorsObj(client, detectorsArray));
            }
        }



        public static Client CreateClient(int i)
        {
            Random r = new Random();
            Calendar myCal = CultureInfo.InvariantCulture.Calendar;
            DateTime myDT = DateTime.Now;
            int password = r.Next(100, 999);
            string name = GetEmail();
            string email = name + i.ToString() + "@gmail.com";
            string phone = "050" + r.Next(1000000, 9999999);
            DateTime time = myCal.AddDays(myDT, r.Next(1, 30));
            string color = Colors[r.Next(0, Colors.Count)];
            Client newClient = new Client(email, email, password.ToString(), name, phone, time, color);
            return newClient;
        }

        public static SmokingDetector CreateDetectors(int device_id, string email)
        {
            Random r = new Random();
            Calendar myCal = CultureInfo.InvariantCulture.Calendar;
            DateTime myDT = DateTime.Now;
            double latitude = Convert.ToInt32(34.86 + Math.Round((r.NextDouble() / 100), 4));
            double longitude = Convert.ToInt32(32.5 + Math.Round((r.NextDouble()), 6));
            Console.WriteLine(longitude + ", " + latitude + "-------------------------------------------------------------");
            string address = "null";
            DateTime time = myCal.AddDays(myDT, r.Next(1, 30));
            SmokingDetector newDetector = new SmokingDetector(email, device_id.ToString(), email + device_id.ToString(), address, "A", longitude.ToString(), latitude.ToString(), time);
            return newDetector;
        }

        public static async Task<Events> CreateEvent(int event_id, string device_id, string email, string latitude, string longitude, Container container)
        {
            Random r = new Random();
            Calendar myCal = CultureInfo.InvariantCulture.Calendar;
            DateTime myDT = DateTime.Now;
            Events newEvent = new Events
            {
                id = event_id.ToString(),
                event_id = event_id,
                device_id = device_id,
                email = email,
                country = "null",
                city = null,
                latitude = latitude,
                longitude = longitude,
                is_false_alarm = (r.NextDouble() > 0.5).ToString(),
                event_details = "Fire",
                num_of_injured = r.Next(1,10).ToString(),
                time = myCal.AddDays(myDT, r.Next(1, 30))
        };
            Console.WriteLine(newEvent.id.ToString());
            try
            {
                // Read the item to see if it exists.  
                ItemResponse<Events> newEventResponse = await container.CreateItemAsync<Events>(newEvent, new PartitionKey(newEvent.email));
            }
            catch (CosmosException ex)
            {

                Console.WriteLine("Error adding item to events table: {0}\n", ex);
            }
            return newEvent;
        }

        public static string GetEmail()
        {
            string SALTCHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder salt = new StringBuilder();
            Random rnd = new Random();
            int limit = rnd.Next(4, 10);
            while (salt.Length < limit)
            { // length of the random string.
                int index = (int)(rnd.NextDouble() * SALTCHARS.Length);
                salt.Append(SALTCHARS[index]);
            }
            string saltStr = salt.ToString();
            return saltStr;
        }

    }
}


