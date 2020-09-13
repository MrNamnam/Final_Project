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

namespace SmokingDetectorFunctions
{
    public class SmokingDetector : TableEntity
    {
        public string device_name { get; set; }
        public string address { get; set; }
        public string version { get; set; }
        public string longitude { get; set; }
        public string latitude { get; set; }


        public SmokingDetector(string email, string device_id, string device_name, string address,
                               string version, string longitude, string latitude)
        {
            this.device_name = device_name;
            this.address = address;
            this.version = version;
            this.longitude = longitude;
            this.latitude = latitude;

            PartitionKey = email;
            RowKey = device_id;
            //Timestamp = last row modified timestamp
        }

        public SmokingDetector()
        {
        }
    }



}

