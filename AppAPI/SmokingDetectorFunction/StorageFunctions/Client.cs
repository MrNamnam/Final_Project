using Microsoft.WindowsAzure.Storage.Table;

namespace SmokingDetectorFunctions
{
    // Summary:         This class inherit from TableEntity.
    //                  each object (row) is a registered client with his personal information
    //                  to see list of devices, check DetectorEntities table
    // PartitionKey:    id
    // RowKey:          email
    public class Client : TableEntity
    {
        public string password { get; set; }
        public string name { get; set; }
        public string phone_number { get; set; } 
        public string favorite_color { get; set; }

        // Constructor
        public Client(string email, string password, string name, string phone_number, string favorite_color)
        {
            this.password = password;
            this.name = name;
            this.phone_number = phone_number;
            this.favorite_color = favorite_color;

            PartitionKey = email;
            RowKey = email;
            //Timestamp = last row modified timestamp
        }

        // Empty constructor
        public Client()
        {
        }
    }
}

