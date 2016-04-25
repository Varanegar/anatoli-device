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
    public class SettingsFragment : Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            var view = inflater.Inflate(Resource.Layout.SettingsLayout, container, false);
            var ipAddress = view.FindViewById<EditText>(Resource.Id.serverIpEditText);
            var saveButton = view.FindViewById<Button>(Resource.Id.button1);
            ipAddress.Text = Anatoli.Framework.AnatoliBase.Configuration.WebService.PortalAddress;
            saveButton.Click += async (s, e) =>
            {
                Anatoli.Framework.AnatoliBase.Configuration.WebService.PortalAddress = ipAddress.Text;
                await Anatoli.Framework.AnatoliBase.Configuration.SaveConfigToFile();
                AnatoliApp.GetInstance().SetFragment<FirstFragment>(new FirstFragment(), "first_fragment");
                await AnatoliApp.GetInstance().ClearDatabase();
                await AnatoliApp.GetInstance().SaveLogoutAsync();
                AlertDialog.Builder alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                alert.SetMessage("به علت تغییر آی پی باید برنامه دوباره راه اندازی شود");
                alert.SetPositiveButton(Resource.String.Ok, delegate
                {
                    AnatoliApp.GetInstance().Activity.Finish();
                });
                alert.Show();
            };

            return view;
        }
    }
}