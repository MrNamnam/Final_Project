using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace SimulatedDevice
{
    class Program
    {
        private const int one_event = 1;
        private const int many_events = 2;

        private const string Device_id = "2";

        private const string Client_email = "bayan@gmail.com";

        private const string DeviceConnectionString = @"HostName=skeletonIotHub.azure-devices.net;DeviceId=MyCDeviceCsdk;SharedAccessKey=YfQTawgMwQbk4FK569aQGx+D928rfWxzrTnTPEz/Vn0=";

        private static readonly DeviceClient Client = DeviceClient.CreateFromConnectionString(DeviceConnectionString);

        static async Task Main(string[] args)
        {
            Console.WriteLine("Please select action number: ");
            Console.WriteLine($"({one_event}) for one event");
            Console.WriteLine($"({many_events}) for many events");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;

            while (true)
            {
                string action = GetSimulatedAction();
                string longitude = "32.0624708";
                string latitude = "34.7633775";

                var data = new { action = action, email = Client_email, device_id = Device_id,  longitude = longitude, latitude = latitude, ActionType = "SpotStateChange" };
                string messageJson = JsonConvert.SerializeObject(data);
                Message message = new Message(Encoding.ASCII.GetBytes(messageJson)) { ContentType = "application/json", ContentEncoding = "utf-8" };

                Console.WriteLine($"Simulating action ({action}).");
                Console.WriteLine($"longitude: ({longitude}) and latitude: ({latitude}).");
                await Client.SendEventAsync(message);
            }
        }

        private static string GetSimulatedAction()
        {
            while (true)
            {
                char keyChar = Console.ReadKey(true).KeyChar;
                if (char.IsNumber(keyChar))
                {
                    int number = (int)char.GetNumericValue(keyChar);
                    if (number == one_event)
                        return "add_one_event";
                    else if (number == many_events)
                    {
                        return "add_many_events";
                    }

                }
            }
        }


    }
}