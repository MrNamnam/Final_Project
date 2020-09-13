using Newtonsoft.Json;


namespace SmokingDetectorFunctions
{


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