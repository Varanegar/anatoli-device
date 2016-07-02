using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AnatoliAndroid.Activities;

namespace AnatoliAndroid.Fragments
{
    public class AnatoliFragment : Android.Support.V4.App.Fragment
    {
        public string Title;
        public override void OnStart()
        {
            base.OnStart();
            AnatoliApp.GetInstance().SetToolbarTitle(Title);
        }
    }
  
}