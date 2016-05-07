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

namespace AnatoliAndroid.Activities
{
    [Activity(Label = "ایگ", Theme = "@style/Theme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            StartActivity(typeof(MainActivity));
            // Create your application here
        }
        protected override void OnStart()
        {
            base.OnStart();
            Java.Lang.Thread.Sleep(1000);
        }

    }
}