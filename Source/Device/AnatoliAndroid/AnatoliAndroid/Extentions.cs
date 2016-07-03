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
using AnatoliAndroid.Fragments;
using Android.Graphics;
using System.Net;
using Anatoli.Framework;
using System.Runtime.ExceptionServices;

namespace AnatoliAndroid
{
    public static class Extentions
    {
        public static void UpdateWidth(this Button button)
        {
            Android.Util.DisplayMetrics metrics = new Android.Util.DisplayMetrics();
            AnatoliAndroid.Activities.AnatoliApp.GetInstance().Activity.WindowManager.DefaultDisplay.GetMetrics(metrics);
            var width = 2 * (metrics.WidthPixels / 4);
            var pixels = metrics.Xdpi * 2;
            width = ((float)width > pixels) ? (int)Math.Round(pixels) : width;
            button.SetWidth(width);
        }
        public static T Cast<T>(this Java.Lang.Object obj) where T : class
        {
            var propertyInfo = obj.GetType().GetProperty("Instance");
            return propertyInfo == null ? null : propertyInfo.GetValue(obj, null) as T;
        }

        public static Bitmap ImageBitmapFromUrl(string url, int timeout)
        {
            try
            {
                var request = HttpWebRequest.Create(url);
                request.Timeout = timeout;
                Bitmap imageBitmap = null;
                using (var response = request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        imageBitmap = BitmapFactory.DecodeStream(stream);
                    }
                }
                return imageBitmap;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static void SendTrace(this Exception exception)
        {
            try
            {
                if (exception != null)
                {
                    if (exception.InnerException != null)
                        ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                    else
                        ExceptionDispatchInfo.Capture(exception).Throw();
                }
            }
            catch (Exception ex)
            {
                HockeyApp.TraceWriter.WriteTrace(ex, false);
            }
        }

        public static bool CheckFirstRun(this Activity context)
        {

            String PREFS_NAME = "MyPrefsFile";
            String PREF_VERSION_CODE_KEY = "version_code";
            int DOESNT_EXIST = -1;


            // Get current version code
            int currentVersionCode = 0;
            try
            {
                currentVersionCode = context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionCode;
            }
            catch
            {
                return true;
            }

            // Get saved version code
            var prefs = context.GetSharedPreferences(PREFS_NAME, FileCreationMode.Private);
            int savedVersionCode = prefs.GetInt(PREF_VERSION_CODE_KEY, DOESNT_EXIST);

            // Check for first run or upgrade
            if (currentVersionCode == savedVersionCode)
            {

                // This is just a normal run
                return false;

            }
            else if (savedVersionCode == DOESNT_EXIST)
            {

                // TODO This is a new install (or the user cleared the shared preferences)
                prefs.Edit().PutInt(PREF_VERSION_CODE_KEY, currentVersionCode).Commit();
                return true;
            }
            else if (currentVersionCode > savedVersionCode)
            {

                // TODO This is an upgrade
                prefs.Edit().PutInt(PREF_VERSION_CODE_KEY, currentVersionCode).Commit();
                return true;
            }
            prefs.Edit().PutInt(PREF_VERSION_CODE_KEY, currentVersionCode).Commit();
            return false;
            // Update the shared preferences with the current version code


        }
    }
}