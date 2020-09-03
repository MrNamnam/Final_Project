using Java.Lang;
using smokeDetector.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace smokeDetector.views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EventsHistory : ContentPage
    {
        private string ListEventsUrl = "https://smokingdetectorfunction.azurewebsites.net/api/ListEvents/";

        public EventsHistory(string email, string device_name)
        {
            InitializeComponent();
            ObservableCollection<Event> Events = new ObservableCollection<Event>();
            string searchApiUrl = ListEventsUrl + email + "/" + device_name;
            var results = Utilities.ExecuteWebRequest(searchApiUrl, method: "GET");
            if (results.StatusCode != HttpStatusCode.OK)
            {
                throw new System.Exception("Unexpected error querying items");
            }
            else
            {
                // Get events history
                var items = results.ResponseAsJArray();
                for (int i = 0; i < items.Count; i++)
                {
                    // Order them in an Event Object
                    string is_false_alarm = items[i]["is_false_alarm"].ToString() != "null" ? items[i]["is_false_alarm"].ToString(): "No Information";
                    string event_details = items[i]["event_details"].ToString() != "null" ?items[i]["event_details"].ToString() : "No Information";
                    string num_of_injured = items[i]["num_of_injured"].ToString() != "null" ? items[i]["num_of_injured"].ToString() : "No Information" ;
                    string time = items[i]["time"] !=null ? items[i]["time"].ToString() : "No Information";

                    Event item = new Event(is_false_alarm, event_details, num_of_injured, time);
                    Events.Add(item);
                }
                listUser.ItemsSource = Events;
            }
        }
    }

    internal class Event
    {
        public string is_false_alarm { get; set; }
        public string event_details { get; set; }
        public string num_of_injured { get; set; }
        public string time { get; set; }
        public string details { get; set; }

        public Event(string is_false_alarm, string event_details, string num_of_injured, string time)
        {
            this.is_false_alarm = is_false_alarm;
            this.event_details = event_details;
            this.num_of_injured = num_of_injured;
            this.time = time;
            string temp = is_false_alarm == "false" ? "Number of injured: "+ num_of_injured : "";
            details = "Details: " +event_details + "\n" + "Is false alarm: " + is_false_alarm + "\n" + temp;              
        }
    }
}