﻿using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Anatoli.App.Manager;
using Android.Net;
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
using HockeyApp;
using Android.Gms.Common;
using Android.Gms.Gcm.Iid;
using Android.Gms.Gcm;


namespace AnatoliAndroid.Activities
{
    [Activity(Label = "ایگ", Icon = "@drawable/icon", ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class MainActivity : AppCompatActivity, ILocationListener
    {
        Toolbar _toolbar;
        LocationManager _locationManager;
        public const string HOCKEYAPP_APPID = "74cf61c0125342949c98afc10b5f9e21";
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var cn = (ConnectivityManager)GetSystemService(ConnectivityService);
            AnatoliClient.Initialize(new AndroidWebClient(cn), new SQLiteAndroid(), new AndroidFileIO());
            CrashManager.Register(this, HOCKEYAPP_APPID, new AnatoliCrashManagerListener());
            TraceWriter.Initialize();
            AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
                {
                    TraceWriter.WriteTrace(args.Exception);
                    args.Handled = true;
                };
            AppDomain.CurrentDomain.UnhandledException +=
            (sender, args) => TraceWriter.WriteTrace(args.ExceptionObject);
            TaskScheduler.UnobservedTaskException +=
                (sender, args) => TraceWriter.WriteTrace(args.Exception);
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

            if (Build.VERSION.SdkInt > Android.OS.BuildVersionCodes.Lollipop)
            {
                Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            }
            _toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(_toolbar);
        }
        protected async override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            AnatoliClient.GetInstance().WebClient.TokenExpire += (s, e) =>
            {
                AnatoliApp.GetInstance().Logout();
                var currentFragmentType = AnatoliApp.GetInstance().GetCurrentFragmentType();
                if (currentFragmentType == typeof(ProfileFragment))
                {
                    AnatoliApp.GetInstance().BackFragment();
                }
                LoginFragment login = new LoginFragment();
                var transaction = AnatoliApp.GetInstance().Activity.FragmentManager.BeginTransaction();
                login.Show(transaction, "login_fragment");
            };
            var user = await AnatoliUserManager.ReadUserInfoAsync();
            AnatoliApp.Initialize(this, user, FindViewById<ListView>(Resource.Id.drawer_list), _toolbar);
            if (IsPlayServicesAvailable())
            {
                var intent = new Intent(this, typeof(RegistrationIntentService));
                StartService(intent);
            }
            _locationManager = (LocationManager)GetSystemService(LocationService);
            AnatoliApp.GetInstance().DrawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            AnatoliApp.GetInstance().LocationManager = _locationManager;
            AnatoliApp.GetInstance().UpdateBasketIcon();
            if (AnatoliApp.GetInstance().AnatoliUser != null)
            {
#pragma warning disable
                try
                {
                    if (AnatoliClient.GetInstance().WebClient.IsOnline())
                    {
                        AnatoliApp.GetInstance().RefreshCutomerProfile();
                    }
                }
                catch (Exception e)
                {
                    e.SendTrace();
                }
#pragma warning restore
            }
            try
            {
                var defaultStore = StoreManager.GetDefault();
                if (defaultStore != null)
                {
                    AnatoliApp.GetInstance().DefaultStore = defaultStore;
                    AnatoliApp.GetInstance().Customer = await CustomerManager.ReadCustomerAsync();
                    AnatoliApp.GetInstance().RefreshMenuItems();
                    AnatoliApp.GetInstance().PushFragment(new FirstFragment(), "first_fragment", true, true);
                }
                else
                {
                    var storesF = new StoresListFragment();
                    storesF.SetQuery(StoreManager.GetAllQueryString());
                    AnatoliApp.GetInstance().PushFragment(storesF, "stores_fragment", true, true);
                }
            }
            catch (Exception)
            {

            }

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
        bool IsPlayServicesAvailable()
        {
            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (resultCode != ConnectionResult.Success)
            {
                if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
                {
                    var alert = new Android.App.AlertDialog.Builder(this);
                    alert.SetTitle(Resource.String.Error);
                    alert.SetMessage("لطفا برنامه google play را نصب کنید");
                    alert.SetPositiveButton(Resource.String.Ok, delegate { });
                    alert.Show();
                    Finish();
                }
                return false;
            }
            else
            {
                return true;
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
    class AnatoliCrashManagerListener : CrashManagerListener
    {
        public override bool ShouldAutoUploadCrashes()
        {
            return true;
        }
    }

    [Service]
    class SyncDataBaseService : Service
    {

        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Console.WriteLine("Service started");
            System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    if (AnatoliClient.GetInstance().WebClient.IsOnline())
                    {
                        await SyncManager.SyncDatabase();
                    }
                }
                catch (System.Net.WebException)
                {

                }
                catch (Exception e)
                {
                    e.SendTrace();
                }
                StopSelf();
            });
            return StartCommandResult.Sticky;
        }

    }

    [Service(Exported = false)]
    class RegistrationIntentService : IntentService
    {
        static object locker = new object();

        public RegistrationIntentService() : base("RegistrationIntentService") { }

        protected override void OnHandleIntent(Intent intent)
        {
            try
            {
                lock (locker)
                {
                    var instanceID = InstanceID.GetInstance(this);
                    var token = instanceID.GetToken(
                        "269775973801", GoogleCloudMessaging.InstanceIdScope, null);
                    SendRegistrationToAppServer(token);
                    Subscribe(token);

                }
            }
            catch (Exception e)
            {
                return;
            }
        }

        void SendRegistrationToAppServer(string token)
        {
            // Add custom implementation here as needed.
        }
        void Subscribe(string token)
        {
            var pubSub = GcmPubSub.GetInstance(this);
            pubSub.Subscribe(token, "/topics/global", null);
        }
    }
}

