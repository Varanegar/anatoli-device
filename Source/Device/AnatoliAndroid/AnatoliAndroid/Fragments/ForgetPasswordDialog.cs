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
using Anatoli.App.Manager;
using Anatoli.Framework.AnatoliBase;

namespace AnatoliAndroid.Fragments
{
    public class ForgetPasswordDialog : DialogFragment
    {
        public string UserName;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Dialog.SetTitle("تغییر رمز عبور");
            Dialog.SetCanceledOnTouchOutside(false);
            var view = inflater.Inflate(Resource.Layout.ForgetPasswordLayout, container, false);
            var editText1 = view.FindViewById<EditText>(Resource.Id.editText1);
            if (!string.IsNullOrEmpty(UserName))
            {
                editText1.Text = UserName;
            }
            var button1 = view.FindViewById<Button>(Resource.Id.button1);
            button1.UpdateWidth();
            button1.Click += async delegate
             {

                 if (!AnatoliClient.GetInstance().WebClient.IsOnline())
                 {
                     AlertDialog.Builder eAlert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                     eAlert.SetMessage("لطفا دستگاه خود را به اینترنت متصل کنید");
                     eAlert.SetPositiveButton(Resource.String.Ok, delegate
                     {
                         Intent intent = new Intent(Android.Provider.Settings.ActionSettings);
                         AnatoliApp.GetInstance().Activity.StartActivity(intent);
                     });
                     eAlert.SetNegativeButton(Resource.String.Cancel, delegate
                     {
                         Dismiss();
                     });
                     eAlert.Show();
                     return;
                 }

                 ProgressDialog pDialog = new ProgressDialog(AnatoliApp.GetInstance().Activity);
                 pDialog.SetMessage(AnatoliApp.GetResources().GetString(Resource.String.PleaseWait));
                 pDialog.Show();
                 try
                 {
                     var result = await AnatoliUserManager.SendPassCode(editText1.Text);
                     pDialog.Dismiss();
                     ResetPasswordDialog resetDialog = new ResetPasswordDialog();
                     resetDialog.UserName = editText1.Text;
                     var t = AnatoliApp.GetInstance().Activity.FragmentManager.BeginTransaction();
                     resetDialog.Show(t, "confirm_dialog");
                     resetDialog.PassWordChanged += delegate
                     {
                         AlertDialog.Builder eAlert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                         eAlert.SetMessage("کلمه عبور با موفقیت تغییر یافت");
                         eAlert.SetPositiveButton(Resource.String.Ok, delegate
                         {
                             Dismiss();
                         });
                         eAlert.Show();
                     };
                     resetDialog.PassWordFailed += (msg) =>
                     {
                         AlertDialog.Builder eAlert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                         eAlert.SetTitle(Resource.String.Error);
                         eAlert.SetMessage(msg);
                         eAlert.SetPositiveButton(Resource.String.Ok, delegate
                         {
                             Dismiss();
                         });
                         eAlert.Show();
                     };
                 }
                 catch (Exception)
                 {
                     pDialog.Dismiss();
                     AlertDialog.Builder eAlert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                     eAlert.SetMessage("اجرای درخواست شما با خطا مواجه شد");
                     eAlert.SetPositiveButton(Resource.String.Ok, delegate
                     {
                         Dismiss();
                     });
                     eAlert.Show();
                 }
                 
                
             };
            return view;
        }
    }
}