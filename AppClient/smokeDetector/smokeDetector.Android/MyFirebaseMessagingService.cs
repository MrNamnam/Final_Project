using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;
using Firebase.Messaging;
using System;
using System.Text.RegularExpressions;
using Android.Util;
using WindowsAzure.Messaging;
using System.Linq;
using System.Collections.Generic;

namespace smokeDetector.Droid
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class MyFirebaseMessagingService : FirebaseMessagingService
    {
        const string TAG = "MyFirebaseMsgService";
        NotificationHub hub;

        public override void OnMessageReceived(RemoteMessage message)
        {
            Log.Debug(TAG, "From: " + message.From);
            if (message.GetNotification() != null)
            {
                //These is how most messages will be received
                Log.Debug(TAG, "Notification Message Body: " + message.GetNotification().Body);
                SendNotification(message.GetNotification().Body);
            }
            else
            {
                //Only used for debugging payloads sent from the Azure portal
                SendNotification(message.Data.Values.First());

            }
        }

        void SendNotification(string messageBody)
        {
            string user = App.Current.Properties.ContainsKey("Email") ? Convert.ToString(App.Current.Properti‌​es["Email"]) : "";
            if (user != "") 
            {
                string email = messageBody.Split(": ")[1];
                messageBody = messageBody.Split(": ")[0];

                if (email == user)
                {
                    var intent = new Intent(this, typeof(MainActivity));
                    intent.AddFlags(ActivityFlags.ClearTop);
                    var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.OneShot);

                    var notificationBuilder = new NotificationCompat.Builder(this, MainActivity.CHANNEL_ID);

                    notificationBuilder.SetContentTitle("Check Devices")
                                .SetSmallIcon(Resource.Drawable.ic_launcher)
                                .SetContentText(messageBody)
                                .SetAutoCancel(true)
                                .SetShowWhen(false)
                                .SetContentIntent(pendingIntent);

                    var notificationManager = NotificationManager.FromContext(this);

                    notificationManager.Notify(0, notificationBuilder.Build());
                }
                
            }
           
        }

        public override void OnNewToken(string token)
        {
            Log.Debug(TAG, "FCM token: " + token);
            SendRegistrationToServer(token);
        }

        void SendRegistrationToServer(string token)
        {
            // Register with Notification Hubs
            hub = new NotificationHub(Constants.NotificationHubName,
                                        Constants.ListenConnectionString, this);

            Registration registration = hub.Register(token, AppConstants.SubscriptionTags);

            // subscribe to the SubscriptionTags list with a simple template.
            string pnsHandle = registration.PNSHandle;
            TemplateRegistration templateReg = hub.RegisterTemplate(pnsHandle, "defaultTemplate", AppConstants.FCMTemplateBody, AppConstants.SubscriptionTags);

        }


        public static class AppConstants
         {
             public static string NotificationChannelName { get; set; } = "XamarinNotifyChannel";
             public static string DebugTag { get; set; } = "XamarinNotify";
             public static string[] SubscriptionTags { get; set; } = { "default" };
             public static string FCMTemplateBody { get; set; } = "{\"data\":{\"message\":\"$(messageParam)\"}}";
             public static string APNTemplateBody { get; set; } = "{\"aps\":{\"alert\":\"$(messageParam)\"}}";
         }
    }
}

