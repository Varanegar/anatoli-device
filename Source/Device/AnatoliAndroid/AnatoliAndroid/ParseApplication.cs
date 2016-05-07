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
using Parse;
using Anatoli.App;
using Anatoli.Framework.AnatoliBase;
using Android.Net;

namespace AnatoliAndroid
{
#if DEBUG
[Application(Debuggable=true , Name = "anatoliandroid.ParseApplication")]
#else
    [Application(Debuggable = false, Name = "anatoliandroid.ParseApplication")]
#endif
    public class ParseApplication : Application
    {
        public ParseApplication(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();
            var cn = (ConnectivityManager)GetSystemService(ConnectivityService);
            AnatoliClient.GetInstance(new AndroidWebClient(cn), new SQLiteAndroid(), new AndroidFileIO());
            if (AnatoliClient.GetInstance().WebClient.IsOnline())
            {
                ParseClient.Initialize(Configuration.parseAppId, Configuration.parseDotNetKey);
                ParsePush.ParsePushNotificationReceived += ParsePush_ParsePushNotificationReceived;
            }

        }

        void ParsePush_ParsePushNotificationReceived(object sender, ParsePushNotificationEventArgs e)
        {
            var data = e.Payload;
            var s = e.StringPayload;

        }
    }
}