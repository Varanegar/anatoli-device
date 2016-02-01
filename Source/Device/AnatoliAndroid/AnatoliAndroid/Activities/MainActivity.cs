﻿using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Anatoli.App.Manager;
using Android.Net;
using Parse;
using AnatoliAndroid.Fragments;
using Android.Support.V4.Widget;
using Android.Support.V4.App;
using System.Collections.Generic;
using Anatoli.Framework.AnatoliBase;
using Android.Support.V7.App;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using AnatoliAndroid.ListAdapters;
using System.Threading.Tasks;
using Android.Locations;
using Anatoli.App;
using Anatoli.App.Model;
using Android.Provider;
using Android.Graphics;
using Android.Database;


namespace AnatoliAndroid.Activities
{
    [Activity(Label = "ایگ مارکت", Icon = "@drawable/icon", ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class MainActivity : ActionBarActivity, ILocationListener
    {
        Toolbar _toolbar;
        LocationManager _locationManager;
        public const string HOCKEYAPP_APPID = "1de510d412d34929b0e5db5c446a9f32";
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            HockeyApp.CrashManager.Register(this, HOCKEYAPP_APPID);
            HockeyApp.TraceWriter.Initialize();
            // Wire up Unhandled Expcetion handler from Android
            AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
                {
                    // Use the trace writer to log exceptions so HockeyApp finds them
                    HockeyApp.TraceWriter.WriteTrace(args.Exception);
                    args.Handled = true;
                };
            AppDomain.CurrentDomain.UnhandledException +=
            (sender, args) => HockeyApp.TraceWriter.WriteTrace(args.ExceptionObject);

            // Wire up the unobserved task exception handler
            TaskScheduler.UnobservedTaskException +=
                (sender, args) => HockeyApp.TraceWriter.WriteTrace(args.Exception);


            SetContentView(Resource.Layout.Main);

            _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver, new IntentFilter(ConnectivityManager.ConnectivityAction));

            _locBroadCastReciever = new LocationManagerBroadcastReceiver();
            _locBroadCastReciever.LocationManagerStatusChanged += (s, e) =>
            {
                AnatoliApp.GetInstance().StartLocationUpdates();
            };
            Application.Context.RegisterReceiver(_locBroadCastReciever, new IntentFilter(LocationManager.ProvidersChangedAction));

            if (Build.VERSION.SdkInt > Build.VERSION_CODES.Lollipop)
            {
                Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            }
            _toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(_toolbar);
        }

        protected async override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);

            var user = await AnatoliUserManager.ReadUserInfoAsync();
            AnatoliApp.Initialize(this, user, FindViewById<ListView>(Resource.Id.drawer_list), _toolbar);
            _locationManager = (LocationManager)GetSystemService(LocationService);
            AnatoliApp.GetInstance().DrawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            AnatoliApp.GetInstance().LocationManager = _locationManager;
            try
            {
                int v = Anatoli.App.Manager.SyncManager.LoadDBVersion();
                if (v == 0)
                {
                    await AnatoliApp.GetInstance().SyncDatabase();
                }
                AnatoliApp.GetInstance().DefaultStore = (await StoreManager.GetDefaultAsync()).store_name;
                AnatoliApp.GetInstance().RefreshMenuItems();
                AnatoliAndroid.Activities.AnatoliApp.GetInstance().ShoppingCardItemCount.Text = (await ShoppingCardManager.GetItemsCountAsync()).ToString();
                AnatoliAndroid.Activities.AnatoliApp.GetInstance().SetTotalPrice(await ShoppingCardManager.GetTotalPriceAsync());
                AnatoliApp.GetInstance().SetFragment<FirstFragment>(null, "first_fragment");
                if (AnatoliApp.GetInstance().AnatoliUser != null)
                {
#pragma warning disable
                    AnatoliApp.GetInstance().RefreshCutomerProfile();
                    BasketManager.SyncDataBase();
#pragma warning restore
                }
            }
            catch (Exception)
            {
                AnatoliApp.GetInstance().SetFragment<StoresListFragment>(null, "stores_fragment");
            }



        }
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            //if (resultCode == Result.Ok)
            //{
            //    Android.Net.Uri selectedImage = data.Data;
            //    String[] filePathColumn = { MediaStore.Images.Media.DefaultSortOrder };
            //    var cursor = ContentResolver.Query(selectedImage, filePathColumn, null, null, null);
            //    if (cursor.MoveToFirst())
            //    {
            //        int columnIndex = cursor.GetColumnIndex(filePathColumn[0]);
            //        String filePath = cursor.GetString(columnIndex);
            //        Bitmap bitmap = BitmapFactory.DecodeFile(filePath);
            //        CustomerManager.UploadImageAsync("aaa", bitmap);
            //    }
            //    cursor.Close();
            //}
        }
        private string GetPathToImage(Android.Net.Uri uri)
        {
            string path = null;
            // The projection contains the columns we want to return in our query.
            string[] projection = new[] { Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data };
            using (ICursor cursor = AnatoliApp.GetInstance().Activity.ManagedQuery(uri, projection, null, null, null))
            {
                if (cursor != null)
                {
                    int columnIndex = cursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data);
                    cursor.MoveToFirst();
                    path = cursor.GetString(columnIndex);
                }
            }
            return path;
        }
        private void OnNetworkStatusChanged(object sender, EventArgs e)
        {


        }


        public override void OnBackPressed()
        {
            if (AnatoliApp.GetInstance().DrawerLayout.IsDrawerOpen(AnatoliApp.GetInstance().DrawerListView))
            {
                AnatoliApp.GetInstance().DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
                return;
            }
            if (AnatoliApp.GetInstance().SearchBarEnabled)
            {
                AnatoliApp.GetInstance().CloseSearchBar();
                return;
            }
            if (!AnatoliApp.GetInstance().BackFragment())
            {
                if (!AnatoliApp.GetInstance().ExitApp)
                {
                    Toast.MakeText(this, "برای خروج دوباره دکمه بازگشت را فشار دهید", ToastLength.Short).Show();
                }
                else
                    Finish();
            }
        }

        public event EventHandler NetworkStatusChanged;
        public NetworkStatusBroadcastReceiver _broadcastReceiver { get; set; }
        public LocationManagerBroadcastReceiver _locBroadCastReciever { get; set; }

        public void OnLocationChanged(Location location)
        {
            AnatoliApp.GetInstance().SetLocation(location);
        }

        public void OnProviderDisabled(string provider)
        {

        }

        public void OnProviderEnabled(string provider)
        {

        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
            if (status == Availability.TemporarilyUnavailable || status == Availability.OutOfService)
            {
                Toast.MakeText(this, provider + "در حال حاضر دسترسی به موقعیت مکانی شما امکان پذیر نمی باشد", ToastLength.Short).Show();
            }
        }



    }

    [BroadcastReceiver()]
    public class NetworkStatusBroadcastReceiver : BroadcastReceiver
    {

        public event EventHandler ConnectionStatusChanged;

        public override void OnReceive(Context context, Intent intent)
        {
            if (ConnectionStatusChanged != null)
                ConnectionStatusChanged(this, EventArgs.Empty);
        }
    }

    [BroadcastReceiver()]
    public class LocationManagerBroadcastReceiver : BroadcastReceiver
    {

        public event EventHandler LocationManagerStatusChanged;

        public override void OnReceive(Context context, Intent intent)
        {
            if (LocationManagerStatusChanged != null)
                LocationManagerStatusChanged(this, EventArgs.Empty);
        }
    }

    public class TestClass
    {
        public string name { get; set; }
    }

}

