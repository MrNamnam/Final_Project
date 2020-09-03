using Microsoft.AspNetCore.SignalR.Client;
using smokeDetector.Helper;
using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace smokeDetector.views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Chat : ContentPage
    {
        private string meme = "10,10,80,10";
        private string chatUrl = "https://smokingdetectorfunction.azurewebsites.net/api/chat/";
        public static  ObservableCollection<Message> messages = new ObservableCollection<Message>();

        public Chat()
        {
            InitializeComponent();
            var send_tap = new TapGestureRecognizer();
            send_tap.Tapped += send_tap_Tapped;
            sendLabel.GestureRecognizers.Add(send_tap);
            listMessage.ItemsSource = messages;

            // Receive Firefighters Message
            UsersListPage.connection.On<string>("Chat", (item) =>
            {
                // Add Firefighters Message
                Message message = new Message(item, "Firefighters", "10,10,80,10", "LightGray", "Black", "true", "false") ;
                messages.Add(message);
            }
            );
        }

        private void send_tap_Tapped(object sender, EventArgs e)
        {
            if (TextEntry.Text != string.Empty)
            {
                //Send User Message
                string Url = chatUrl + TextEntry.Text + "/" + UsersListPage.Email;
                var result = Utilities.ExecuteWebRequest(Url, method: "GET");
                // Add User Message 
                Message message = new Message(TextEntry.Text, "You", "80,10,10,10", "LightGreen", "White", "false", "true");
                messages.Add(message);
                TextEntry.Text = string.Empty;
            }
            else
            {
            }
           
        }
    }

    public class Message
    {
        public string text { get; set; }
        public string sentFrom { get; set; } 
        public string margin { get; set; }
        public string BackgroundColor { get; set; }
        public string textColor { get; set; }
        public string isFirefighters { get; set; }
        public string isYou{ get; set; }

    public Message(string text, string sentFrom, string margin , string BackgroundColor, string textColor, string isFirefighters, string isYou)
        {
            this.text = text;
            this.sentFrom = sentFrom;
            this.margin = margin;
            this.BackgroundColor = BackgroundColor;
            this.textColor = textColor;
            this.isFirefighters = isFirefighters;
            this.isYou = isYou;


        }
    }
}