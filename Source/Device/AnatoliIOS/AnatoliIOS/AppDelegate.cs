using Foundation;
using UIKit;
using AnatoliIOS;
using HockeyApp;
using System;
using System.Threading.Tasks;
using Anatoli.Framework.AnatoliBase;
using AnatoliIOS.Clients;

namespace AnatoliIOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        // class-level declarations
        public RootViewController RootViewController { get { return Window.RootViewController as RootViewController; } }
        public override UIWindow Window
        {
            get;
            set;
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            // Override point for customization after application launch.
            // If not required for your application you can safely delete this method

            HockeyApp.Setup.EnableCustomCrashReporting(() =>
            {

                //Get the shared instance
                var manager = BITHockeyManager.SharedHockeyManager;

                //Configure it to use our APP_ID
                manager.Configure("2593c24acb0c42d9af831e611b40b752");

                //Start the manager
                manager.StartManager();

                //Authenticate (there are other authentication options)
                manager.Authenticator.AuthenticateInstallation();

                //Rethrow any unhandled .NET exceptions as native iOS 
                // exceptions so the stack traces appear nicely in HockeyApp
                AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                    Setup.ThrowExceptionAsNative(e.ExceptionObject);

                TaskScheduler.UnobservedTaskException += (sender, e) =>
                    Setup.ThrowExceptionAsNative(e.Exception);
            });


            UINavigationBar.Appearance.SetTitleTextAttributes(new UITextAttributes()
            {
                TextColor = UIColor.White,
                TextShadowColor = UIColor.LightGray,
                Font = UIFont.FromName("IRAN", 15f)
            });
            UIBarButtonItem.Appearance.SetTitleTextAttributes(new UITextAttributes()
            {
                TextColor = UIColor.White,
                TextShadowColor = UIColor.LightGray,
                Font = UIFont.FromName("IRAN", 15f)
            }, UIControlState.Normal);
            UINavigationBar.Appearance.BarTintColor = UIColor.Clear.FromHex(0x085e7d);
            UINavigationBar.Appearance.TintColor = UIColor.White;

            var defaults = NSUserDefaults.StandardUserDefaults;
            const string key = "CurrentVersion";
            double latestVersion = defaults.DoubleForKey(key);
            string version = NSBundle.MainBundle.InfoDictionary["CFBundleVersion"].ToString();
            double currentVersion = Double.Parse(version);
            AnatoliClient.GetInstance(new IosWebClient(), new IosSqliteClient(latestVersion, currentVersion), new IosFileIO());
            defaults.SetDouble(currentVersion, key);
            defaults.Synchronize();


            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                var pushSettings = UIUserNotificationSettings.GetSettingsForTypes(
                                   UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound,
                                   new NSSet());

                UIApplication.SharedApplication.RegisterUserNotificationSettings(pushSettings);
                UIApplication.SharedApplication.RegisterForRemoteNotifications();
            }
            else
            {
                UIRemoteNotificationType notificationTypes = UIRemoteNotificationType.Alert | UIRemoteNotificationType.Badge | UIRemoteNotificationType.Sound;
                UIApplication.SharedApplication.RegisterForRemoteNotificationTypes(notificationTypes);
            }

            return true;
        }

        public override void OnResignActivation(UIApplication application)
        {
            // Invoked when the application is about to move from active to inactive state.
            // This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
            // or when the user quits the application and it begins the transition to the background state.
            // Games should use this method to pause the game.
        }

        public override void DidEnterBackground(UIApplication application)
        {
            // Use this method to release shared resources, save user data, invalidate timers and store the application state.
            // If your application supports background exection this method is called instead of WillTerminate when the user quits.
        }

        public override void WillEnterForeground(UIApplication application)
        {
            // Called as part of the transiton from background to active state.
            // Here you can undo many of the changes made on entering the background.
        }

        public override void OnActivated(UIApplication application)
        {
            // Restart any tasks that were paused (or not yet started) while the application was inactive. 
            // If the application was previously in the background, optionally refresh the user interface.
        }

        public override void WillTerminate(UIApplication application)
        {
            // Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
        }

        public async override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            // Get current device token
            var DeviceToken = deviceToken.Description;
            if (!string.IsNullOrWhiteSpace(DeviceToken))
            {
                DeviceToken = DeviceToken.Trim('<').Trim('>');
            }

            // Get previous device token
            var oldDeviceToken = NSUserDefaults.StandardUserDefaults.StringForKey("PushDeviceToken");

            // Has the token changed?
            if (string.IsNullOrEmpty(oldDeviceToken) || !oldDeviceToken.Equals(DeviceToken))
            {
                //TODO: Put your own logic here to notify your server that the device token has changed/been created!
                var result = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<string>("http://parastoo.varanegar.com:9192/", TokenType.AppToken, "/api/notification/registerApnToken/", false, new Tuple<string, string>("appToken", DeviceToken));
            }

            // Save new device token 
            NSUserDefaults.StandardUserDefaults.SetString(DeviceToken, "PushDeviceToken");
        }
        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            //new UIAlertView("Error registering push notifications", error.LocalizedDescription, null, "OK", null).Show();
        }
        public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
        {
            Console.WriteLine("Notification recieved");
            //if (null != userInfo && userInfo.ContainsKey(new NSString("name")))
            //{
            //    string alert = (userInfo[new NSString("name")] as NSString).ToString();
            //    if (!string.IsNullOrEmpty(alert))
            //    {
            //        UIAlertView avAlert = new UIAlertView("Notification", alert, null, "OK", null);
            //        avAlert.Show();
            //    }
            //}
            //else
            //{
            //    UIAlertView avAlert = new UIAlertView("Notification", "WWWWWWW", null, "OK", null);
            //    avAlert.Show();
            //}
        }
    }
}