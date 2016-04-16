using System;
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
using HockeyApp;


namespace AnatoliAndroid.Activities
{
    [Activity(Label = "ایگ", Icon = "@drawable/icon", ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class MainActivity : ActionBarActivity, ILocationListener
    {
        Toolbar _toolbar;
        LocationManager _locationManager;
        public const string HOCKEYAPP_APPID = "74cf61c0125342949c98afc10b5f9e21";
        //public static readonly int OpenImageRequestCode = 1234;
        protected override void OnSaveInstanceState(Bundle outState)
        {

        }
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            HockeyApp.CrashManager.Register(this, HOCKEYAPP_APPID, new AnatoliCrashManagerListener());
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

            AnatoliClient.GetInstance().WebClient.TokenExpire += async (s, e) =>
            {
                await AnatoliApp.GetInstance().SaveLogoutAsync();
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
            _locationManager = (LocationManager)GetSystemService(LocationService);
            AnatoliApp.GetInstance().DrawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            AnatoliApp.GetInstance().LocationManager = _locationManager;
            //await ProductManager.SyncProductTagsAsync();
            try
            {
                var updateTime = await SyncManager.GetLogAsync(SyncManager.UpdateCompleted);
                if (updateTime == DateTime.MinValue)
                {
                    await AnatoliApp.GetInstance().SyncDatabase();
                }
                var latestUpdateTime = await SyncManager.GetLogAsync(SyncManager.OnHand);
                if ((DateTime.Now - latestUpdateTime).TotalMinutes > 10)
                {
                    StartService(new Intent(this, typeof(StockFeedService)));
                }
                var defaultStore = await StoreManager.GetDefaultAsync();
                if (defaultStore != null)
                {
                    AnatoliApp.GetInstance().SetDefaultStore(defaultStore);
                    AnatoliApp.GetInstance().Customer = await CustomerManager.ReadCustomerAsync();
                    AnatoliApp.GetInstance().RefreshMenuItems();
                    AnatoliAndroid.Activities.AnatoliApp.GetInstance().ShoppingCardItemCount.Text = (await ShoppingCardManager.GetItemsCountAsync()).ToString();
                    AnatoliAndroid.Activities.AnatoliApp.GetInstance().SetTotalPrice(await ShoppingCardManager.GetTotalPriceAsync());
                    AnatoliApp.GetInstance().SetFragment<FirstFragment>(new FirstFragment(), "first_fragment");
                    if (AnatoliApp.GetInstance().AnatoliUser != null)
                    {
#pragma warning disable
                        try
                        {
                            await OrderManager.SyncOrdersAsync(AnatoliApp.GetInstance().CustomerId);
                            AnatoliApp.GetInstance().RefreshCutomerProfile();
                            ProductManager.SyncFavoritsAsync();
                            Configuration.ReadConfigFromFile();
                        }
                        catch (Exception e)
                        {
                            e.SendTrace();
                        }
#pragma warning restore
                    }

                }
                else
                {
                    var storesF = AnatoliApp.GetInstance().SetFragment<StoresListFragment>(new StoresListFragment(), "stores_fragment");
                    await storesF.RefreshAsync();
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

        //protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        //{
        //    base.OnActivityResult(requestCode, resultCode, data);

        //}
        //void OnImageUploaded()
        //{
        //    if (ImageUploaded != null)
        //    {
        //        ImageUploaded.Invoke(this, new EventArgs());
        //    }
        //}
        //public EventHandler ImageUploaded;

        //void OnImageUploadFailed()
        //{
        //    if (ImageUploadFailed != null)
        //    {
        //        ImageUploadFailed.Invoke(this, new EventArgs());
        //    }
        //}
        //public EventHandler ImageUploadFailed;
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

    class AnatoliCrashManagerListener : CrashManagerListener
    {
        public override bool ShouldAutoUploadCrashes()
        {
            return true;
        }
    }

    [Service]
    class StockFeedService : Service
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
                    await ProductManager.SyncOnHandAsync(null);
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
}

