using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmokingDetectorFunction
{
    public class CurrentAlerts : TableEntity
    {
        public string email { get; set; }
        public string device_id { get; set; }
        public string longitude { get; set; }
        public string latitude { get; set; }
        public DateTime time { get; set; }


        public CurrentAlerts(string email, string device_id, string longitude, string latitude, DateTime time)
        {
            this.email = email;
            this.device_id = device_id;
            this.longitude = longitude;
            this.latitude = latitude;
            this.time = time;

            PartitionKey = email;
            RowKey = device_id;
            //Timestamp = last row modified timestamp
        }

        public CurrentAlerts()
        {
        }
    }
}
