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

namespace AnatoliAndroid.Fragments
{
    public class ResetPasswordDialog : DialogFragment
    {
        public string UserName;
        public string PassWord;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var view = inflater.Inflate(Resource.Layout.ResetPassWordLayout, container, false);

            EditText codeEditText = view.FindViewById<EditText>(Resource.Id.codeEditText);
            var editText2 = view.FindViewById<EditText>(Resource.Id.editText2);
            var editText3 = view.FindViewById<EditText>(Resource.Id.editText3);

            Button okButton = view.FindViewById<Button>(Resource.Id.okButton);
            okButton.UpdateWidth();
            Dialog.SetTitle(Resource.String.ChangePassword);
            Dialog.SetCanceledOnTouchOutside(false);

            okButton.Click += async delegate
            {
                if (string.IsNullOrEmpty(editText2.Text))
                {
                    var alert = new AlertDialog.Builder(Activity);
                    alert.SetTitle(Resource.String.Error);
                    alert.SetMessage("لطفا کلمه عبور را وارد کنید");
                    alert.SetPositiveButton(Resource.String.Ok, delegate { });
                    alert.Show();
                    return;
                }
                if (string.IsNullOrEmpty(editText3.Text))
                {
                    var alert = new AlertDialog.Builder(Activity);
                    alert.SetTitle(Resource.String.Error);
                    alert.SetMessage("لطفا تکرار کلمه عبور را وارد کنید");
                    alert.SetPositiveButton(Resource.String.Ok, delegate { });
                    alert.Show();
                    return;
                }
                if (!editText2.Text.Equals(editText3.Text))
                {
                    var alert = new AlertDialog.Builder(Activity);
                    alert.SetTitle(Resource.String.Error);
                    alert.SetMessage("کلمه عبور و تکرار آن یکسان نیستند");
                    alert.SetPositiveButton(Resource.String.Ok, delegate { });
                    alert.Show();
                    return;
                }

                ProgressDialog pDialog = new ProgressDialog(AnatoliApp.GetInstance().Activity);
                pDialog.SetMessage(Resources.GetString(Resource.String.PleaseWait));
                pDialog.Show();
                try
                {
                    var result = await AnatoliUserManager.ResetPasswordByCode(UserName, editText3.Text.Trim(), codeEditText.Text.Trim());
                    pDialog.Dismiss();
                    if (result.IsValid)
                        OnPassWordChanged();
                    else
                        OnPassWordFailed(result.message);
                }
                catch (Exception)
                {
                    OnPassWordFailed("خطا!");
                }
                finally
                {
                    pDialog.Dismiss();
                    Dismiss();
                }
            };

            return view;
        }
        void OnPassWordChanged()
        {
            if (PassWordChanged != null)
            {
                PassWordChanged.Invoke(this, new EventArgs());
            }
        }
        public EventHandler PassWordChanged;
        void OnPassWordFailed(string msg)
        {
            if (PassWordFailed != null)
            {
                PassWordFailed.Invoke(msg);
            }
        }
        public event PassWordFailedEventHandler PassWordFailed;
        public delegate void PassWordFailedEventHandler(string msg);
    }
}