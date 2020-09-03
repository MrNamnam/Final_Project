using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace smokeDetector.Droid
{
    class Constants
    {
        public const string ListenConnectionString = "Endpoint=sb://smokedetectornamespace.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=lcSi94dTw+K6GG9jOzCrrBtL+VJgwNEu7PE8oT2DKUc=";
        public const string NotificationHubName = "SmokeDetectorNot";
    }
}