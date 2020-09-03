using System;
using System.Collections.ObjectModel;
using System.Net;
using Xamarin.Forms.Maps;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using smokeDetector.Helper;
using System.Linq;
using Plugin.Geolocator;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System.Net.Http;
using Java.Nio.FileNio;

namespace smokeDetector.views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UsersListPage : ContentPage
    {
        public static HubConnection connection;
        private string ListDevicesUrl = "https://smokingdetectorfunction.azurewebsites.net/api/ListDevices/";
        private string AddDeviceUrl = "https://smokingdetectorfunction.azurewebsites.net/api/AddDevice/";
        private string DeleteDeviceUrl = "https://smokingdetectorfunction.azurewebsites.net/api/DeleteDevice/";
        private string negotiateUrl = "https://smokingdetectorfunction.azurewebsites.net/api/negotiate";
        private string GetDeviceUrl = "https://smokingdetectorfunction.azurewebsites.net/api/GetDevice/";
        private string FalseAlarmUrl = "https://smokingdetectorfunction.azurewebsites.net/api/FalseAlarm/";
        private string clientGetCurrentAlertsUrl = "https://smokingdetectorfunction.azurewebsites.net/api/client_get_current_alerts/";
        static Map map = null;
        public static ObservableCollection<SmokingDetector2> devices = new ObservableCollection<SmokingDetector2>();
        public static string Email = "";
        private Chat chatPage = null; 
        private int _chatButton = 0;
        public int chatButton { get => _chatButton; set => SetProperty(ref _chatButton, value, "chatButton"); }
        public string ischatButton = "false";

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
       
        public   UsersListPage()
        {
            InitializeComponent();
            NavigationPage.SetHasBackButton(this, false);
            //  Register To SignalR
            string body = $@"{{""UserId"":""{Email}""}}";
            string searchApiUrl = negotiateUrl;
            var results = Utilities.ExecuteWebRequest(searchApiUrl, content: body, method: "POST");
            if (results.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Unexpected error querying items");
            }

            // get the results as a Json object
            var jsonItemsResults = results.ResponseAsJObject();
            string url = jsonItemsResults["url"].ToString();
            string token = jsonItemsResults["accessToken"].ToString();
            connection =  new HubConnectionBuilder()
             .WithUrl(url, options =>
             {
                 options.AccessTokenProvider = async () => await Task.FromResult(token);
             })
             .Build();

            // When New Alert Received
            connection.On<CurrentAlerts>("NewAlertApp", Alert);

            // The Request Was Processed
            connection.On<string>("DoneWithAlert", DoneWithAlert);

            try 
            {
                Task.Run(async () => { await connection.StartAsync(); });
            }
            catch (Exception ex)
            {
                // messagesList.Items.Add(ex.Message);

            }

            NavigationPage.SetHasBackButton(this, false);
            new Command(() => addDeviceEntry.Focus());
            addDeviceEntry.ReturnCommand = new Command(() => addDeviceAddress.Focus());

            //Get User Devices
             getDevices();
            chatPage = new Chat();
        }


       /* 
        * inputs: item = device id 
        * 
        */
        public void DoneWithAlert (string item)
        {
            foreach (var device in devices)
            {
                if (device.device_id == item)
                {
                    if (device.falseALarmButton != 0)
                    {
                        device.falseALarmButton = 0;
                        chatButton--;
                    }

                }
                if (chatButton == 0)
                {
                    Chat.messages.Clear();
                    chat.IsVisible = false;
                }
            }
        }

        /*
         * inputs: item = device Alert
         * 
         */
        public  void Alert(CurrentAlerts item)
        {

            string id = item.RowKey.ToString();
            string Url = GetDeviceUrl + Email + "/" + item.RowKey.ToString();
            var result = Utilities.ExecuteWebRequest(Url, method: "GET");
            var jsonItemsResult = result.ResponseAsJObject();
            string device_name = jsonItemsResult["device_name"].ToString();

            foreach (SmokingDetector2 x in devices)
            {
                if (x.device_name == device_name)
                {
                    if (x.falseALarmButton == 0)
                    {
                        chatButton++;
                    }
                    x.device_id = id;
                    x.falseALarmButton = 1;
                    chat.IsVisible = true;

                }

            }
        }

        /*
         * input: pin = pin to add to map
         *  Ged Device Location
        */

        private async Task<string> getLocation(Pin pin)
        {
           
            var locator = CrossGeolocator.Current;
            locator.DesiredAccuracy = 50;
            var position = await locator.GetPositionAsync(TimeSpan.FromSeconds(10000));
            pin.Position = new Position(position.Latitude, position.Longitude);
            // Cast From position To Address
            var address = await locator.GetAddressesForPositionAsync(position);
            pin.Address = $"{address.FirstOrDefault().SubAdminArea} {address.FirstOrDefault().Locality} {address.FirstOrDefault().CountryName}";
            
            return "Done!";
        
          }

        /*
         * Get User Devices 
         */
        private void getDevices()
        {
            // Get User Devices
            string searchApiUrl = ListDevicesUrl + Email;
            devices.Clear();
            var results = Utilities.ExecuteWebRequest(searchApiUrl, method: "GET");
            if (results.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Unexpected error querying items");
            }
            else
            {
                var items = results.ResponseAsJArray();
                // Order devices in a SmokingDetector2 object
                for (int i = 0; i < items.Count; i++)
                {
                    string deviceName = items[i]["device_name"].ToString();
                    string address = items[i]["address"].ToString();
                    double Latitude = double.Parse(items[i]["latitude"].ToString());
                    double Longitude = double.Parse(items[i]["longitude"].ToString());

                    SmokingDetector2 device = new SmokingDetector2(deviceName, address, Latitude, Longitude);
                    devices.Add(device);

                }

                // Get Current Alert Devices
                searchApiUrl = clientGetCurrentAlertsUrl + Email;
                results = Utilities.ExecuteWebRequest(searchApiUrl, method: "GET");
                if (results.StatusCode != HttpStatusCode.OK)
                { 
                    throw new Exception("Unexpected error querying items");
                }
                else
                {
                    items = results.ResponseAsJArray();
                    for (int i = 0; i < items.Count; i++)
                    {
                        string id = items[i].ToString();
                        string Url = GetDeviceUrl + Email + "/" + id;
                        var result = Utilities.ExecuteWebRequest(Url, method: "GET");
                        var jsonItemsResult = result.ResponseAsJObject();
                        string device_name = jsonItemsResult["device_name"].ToString();

                        foreach (var x in devices)
                        {
                            if (x.device_name == device_name)
                            {
                                x.device_id = id;
                                x.falseALarmButton = 1;
                                chatButton++;
                            }
                        }
                    }                   
                }

                if (chatButton > 0)
                {
                    chat.IsVisible = true;

                }

                listUser.ItemsSource = devices;
            }
        }

        /*
         * Send FalseAlarm to Firefighters 
         */
        private void yesFalseAlarm_Clicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            StackLayout listViewItem = (StackLayout)button.Parent;
            Label label = (Label)listViewItem.Children[0];
            string id = "";
            String text = label.Text;
            foreach (var X in devices)
            {
                if(X.device_name == text)
                {
                    if (X.falseALarmButton != 0)
                    {
                        id = X.device_id;
                        string Url = FalseAlarmUrl + Email + "/" + id;
                        var result = Utilities.ExecuteWebRequest(Url, method: "GET");
                    }                    
                }
            }            
        }

        /*
         * Open the chat to talk with Firefighters
         */
        private async void chat_Clicked(object sender, EventArgs e)
        {
            if (chatPage == null)
            {
                chatPage = new Chat();
            }
            await Navigation.PushAsync(chatPage);
            
        }

        /*
         * Get Events History to selected devicce
         */
        private async void GetEventsHistory_Cliced(object sender, EventArgs e)
        {
            if (listUser.SelectedItem == null)
                await DisplayAlert("Notification", "Please select device", "OK");
            else
            {
                SmokingDetector2 device = listUser.SelectedItem as SmokingDetector2;

                await Navigation.PushAsync(new EventsHistory(Email,device.device_name));
            }
        }

        // Get Map
        private async void GetMap_Clicked(object sender, EventArgs e)
        {

            if (map == null)
            {
              await createMap();

            }
            await Navigation.PushAsync(new Maps(map));
        }

        // Creat User Map
        private async Task<string> createMap()
        {
            var locator = CrossGeolocator.Current;
            locator.DesiredAccuracy = 50;
            var position = await locator.GetPositionAsync(TimeSpan.FromSeconds(10000));
            map = new Map(MapSpan.FromCenterAndRadius(new Position(position.Latitude, position.Longitude), Distance.FromMiles(10)));
            for (int x = 0; x < devices.Count; x++)
            {
                var pin = new Pin()
                {
                    Position = new Position(devices[x].latitude, devices[x].longitude),
                    Label = devices[x].device_name
                };
                map.Pins.Add(pin);

            }
            return "Done!";

        }

        // Clear User Param and Logout
        private async void MainPageButton_Clicked(object sender, EventArgs e)
        {
            UsersListPage.Email = "";
            devices.Clear();
            chatPage = null;
            Application.Current.Properties["IsLoggedIn"] = Boolean.FalseString;
            Application.Current.Properties["Email"] = UsersListPage.Email;
            await App.Current.SavePropertiesAsync();
            await Navigation.PushAsync(new MainPage());
        }

        // Delete Selected Device
        private async void DeleteDevice_Clicked(object sender, EventArgs e)
        {

            if (listUser.SelectedItem == null)
                await DisplayAlert("Notification", "Please select device", "OK");
            else
            {
                SmokingDetector2 device = listUser.SelectedItem as SmokingDetector2;
                string searchApiUrl = DeleteDeviceUrl + Email + "/" + device.device_name.ToString();
                listUser.SelectedItem = null;
                var results = Utilities.ExecuteWebRequest(searchApiUrl, method: "GET");
                var retrunvalue = results.ResponseContents;

                if (results.StatusCode != HttpStatusCode.OK)
                {
                    await DisplayAlert("Device Deleted", retrunvalue, "OK");
                }
                else
                {
                    await DisplayAlert("Device Deleted", retrunvalue, "OK");
                    for (int i = 0; i < devices.Count; i++)
                    {
                        if(devices[i].device_name == device.device_name.ToString())
                        {
                            devices.Remove(devices[i]);
                            break;
                        }
                    }
                    for (int i = 0; i < map.Pins.Count; i++)
                    {
                        if (map.Pins[i].Label == device.device_name.ToString())
                        {
                            map.Pins.Remove(map.Pins[i]);
                            break;
                        }

                    }
                }

            }

        }

        //Add New Device
        private async void AddDevice_Clicked(object sender, EventArgs e)
        {
            listUser.SelectedItem = null;
            if (string.IsNullOrWhiteSpace(addDeviceEntry.Text) || string.IsNullOrWhiteSpace(addDeviceAddress.Text))
                await DisplayAlert("Enter Data", "Enter Valid Data", "OK");
            else if (addDeviceAddress.Text.Contains(',') || addDeviceAddress.Text.Contains(':') || addDeviceAddress.Text.Contains('{'))
            {
                await DisplayAlert("Enter Data", "Address should not contains ',', '{', ':'", "OK");
            }
            else
            {
                if (map == null)
                {
                    await createMap();

                }
                var pin = new Pin()
                {
                    Label = addDeviceEntry.Text
                };
                await getLocation(pin);
            map.Pins.Add(pin);
            SmokingDetector2 device = new SmokingDetector2(addDeviceEntry.Text, addDeviceAddress.Text +" " + pin.Address, pin.Position.Latitude, pin.Position.Longitude);
            string searchApiUrl = AddDeviceUrl + Email + "/" + addDeviceEntry.Text + "/" + addDeviceAddress.Text +" " +  pin.Address + "/A" +
            "/" + pin.Position.Longitude.ToString() + "/" + pin.Position.Latitude.ToString();
            var results = Utilities.ExecuteWebRequest(searchApiUrl, method: "GET");
            var retrunvalue = results.ResponseContents;

                if (results.StatusCode != HttpStatusCode.OK)
                {
                    await DisplayAlert("Device Add", retrunvalue, "OK");
                }
                else
                {
                    await DisplayAlert("Device Add", retrunvalue, "OK");
                    
                    devices.Add(device);
                    addDeviceEntry.Text = string.Empty;
                    addDeviceAddress.Text = string.Empty;

                }
            }
        }
    }

    public class SmokingDetector2 : INotifyPropertyChanged
    {
        public string device_name { get; set; }
        public string device_id { get; set; }
        public string address { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        private int _falseALarmButton;
        public  int falseALarmButton { get => _falseALarmButton; set => SetProperty(ref _falseALarmButton, value, "falseALarmButton"); }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public SmokingDetector2(string device_name, string address, double latitude, double longitude)
        {
            this.address = address;
            this.device_name = device_name;
            this.latitude = latitude;
            this.longitude = longitude;
            this.falseALarmButton = 0;
            this.device_id = "";
        }
    }


    public class CurrentAlerts : TableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string longitude { get; set; }
        public string latitude { get; set; }
        public DateTime time { get; set; }


        public CurrentAlerts(string PartitionKey, string RowKey, string longitude, string latitude, DateTime time)
        {
            this.PartitionKey = PartitionKey;
            this.RowKey = RowKey;
            this.longitude = longitude;
            this.latitude = latitude;
            this.time = time;
        }

        public CurrentAlerts()
        {
        }
    }

}
