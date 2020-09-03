using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace signalR_and_web
{

    public class CurrentAlerts : TableEntity
    {
        public DateTime time { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }

        public CurrentAlerts(string email, string device_id, string latitude, string longitude, DateTime time)
        {
            this.latitude = latitude;
            this.longitude = longitude;
            this.time = time;
            this.PartitionKey = email;
            this.RowKey = device_id;
        }

        public CurrentAlerts()
        {
        }
    }


    public class SmokingDetector : TableEntity
    {
        // public string email { get; set; }
        public DateTime time { get; set; }
        public string device_name { get; set; }
        public string address { get; set; }
        public string version { get; set; }
        public string longitude { get; set; }
        public string latitude { get; set; }
        /* Changed delete email and so only in parti and row*/

        public SmokingDetector(string email, string device_id, string device_name, string address,
                               string version, string longitude, string latitude, DateTime time)
        {
            //this.email = email;
            this.device_name = device_name;
            this.address = address;
            this.version = version;
            this.longitude = longitude;
            this.latitude = latitude;
            this.time = time;

            PartitionKey = email;
            RowKey = device_id;
            //Timestamp = last row modified timestamp
        }

        public SmokingDetector()
        {
        }
    }


    // RowKey:          email
    public class Client : TableEntity
    {
        //public string email { get; set; }
        public DateTime time { get; set; }
        public string password { get; set; }
        public string name { get; set; }
        public string phone_number { get; set; }

        // Constructor
        public Client(string email, string password, string name, string phone_number, DateTime time)
        {
            //this.email = email;
            this.password = password;
            this.name = name;
            this.phone_number = phone_number;
            this.time = time;

            PartitionKey = email;
            RowKey = email;
            //Timestamp = last row modified timestamp
        }

        // Empty constructor
        public Client()
        {
        }
    }

    public class ClientDetectorsObj
    {
        public Client client { get; set; }
        public List<SmokingDetector> detectors { get; set; }

        public ClientDetectorsObj(Client client, List<SmokingDetector> detectors)
        {
            this.client = client;
            this.detectors = detectors;
        }
    }



    // For each day in Events table we will get number of events occur that day.
    // We will use it for grapgh in the website
    public class EventdayCount
    {

        public DateTime x { get; set; }
        public int y { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

    }


    // We will count how many time each number of injurd people apear in the events table.
    // We will use it for grapgh in the website
    public class NumOfInjuredCount
    {

        public string label { get; set; }
        public int y { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

    }

    public class CityCount
    {

        public string name { get; set; }
        public int y { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

    }


    //rowkey- id
    public class Events
    {
        [JsonProperty(PropertyName = "id")]
        public string id { get; set; }
        public int event_id { get; set; }
        public string device_id { get; set; }
        public string email { get; set; }
        public string country { get; set; }
        public string city { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string is_false_alarm { get; set; }
        public string event_details { get; set; }
        public string num_of_injured { get; set; }
        public string time { get; set; }


        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
 


    



}
