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
using AnatoliAndroid.Activities;
using Anatoli.App.Manager;
using Anatoli.App.Model.AnatoliUser;
using Anatoli.Framework.AnatoliBase;
using Anatoli.Framework;

namespace AnatoliAndroid.Fragments
{
    [FragmentTitle("ورود")]
    public class ChangePassFragment : DialogFragment
    {
        EditText _currentPassEditText;
        EditText _passwordEditText;
        Button _saveButton;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var view = inflater.Inflate(Resource.Layout.ChangePassLayout, container, false);

            Dialog.SetCanceledOnTouchOutside(false);

            _currentPassEditText = view.FindViewById<EditText>(Resource.Id.currentPassEditText);
            _passwordEditText = view.FindViewById<EditText>(Resource.Id.passwordEditText);
            _saveButton = view.FindViewById<Button>(Resource.Id.saveButton);
            _saveButton.Click += async (s, e) =>
            {
                if (!AnatoliClient.GetInstance().WebClient.IsOnline())
                {
                    AlertDialog.Builder alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                    alert.SetTitle(Resource.String.Error);
                    alert.SetMessage(Resource.String.PleaseConnectToInternet);
                    alert.SetPositiveButton(Resource.String.Ok, delegate { });
                    return;
                }
                if (string.IsNullOrEmpty(_currentPassEditText.Text))
                {
                    AlertDialog.Builder alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                    alert.SetTitle(Resource.String.Error);
                    alert.SetMessage("لطفا کلمه عبور فعلی را وارد نمایید");
                    alert.SetPositiveButton(Resource.String.Ok, delegate { });
                    return;
                }
                if (string.IsNullOrEmpty(_passwordEditText.Text))
                {
                    AlertDialog.Builder alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                    alert.SetTitle(Resource.String.Error);
                    alert.SetMessage("لطفا کلمه عبور جدید را وارد کنید");
                    alert.SetPositiveButton(Resource.String.Ok, delegate { });
                    return;
                }
                ProgressDialog pDialog = new ProgressDialog(AnatoliApp.GetInstance().Activity);
                pDialog.SetMessage("در حال ارسال درخواست");
                pDialog.Show();
                try
                {
                    var result = await AnatoliUserManager.ChangePassword(_currentPassEditText.Text, _passwordEditText.Text);
                    pDialog.Dismiss();
                    if (result != null)
                    {
                        if (result.IsValid)
                        {
                            AlertDialog.Builder alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                            alert.SetMessage("کلمه عبور با موفقیت تغییر کرد");
                            alert.SetPositiveButton(Resource.String.Ok, delegate { Dismiss(); });
                            alert.Show();
                        }
                        else
                        {
                            AlertDialog.Builder alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                            alert.SetMessage("تغییر کلمه عبور با خطا مواجه شد");
                            alert.SetPositiveButton(Resource.String.Ok, delegate { });
                            alert.Show();
                        }
                    }
                }
                catch (AnatoliWebClientException ex)
                {
                    AlertDialog.Builder alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                    alert.SetTitle("خطا");
                    alert.SetMessage(ex.MetaInfo.ModelStateString);
                    alert.SetPositiveButton(Resource.String.Ok, delegate { });
                    alert.Show();
                }
                catch (Exception)
                {
                    AlertDialog.Builder alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                    alert.SetTitle("خطا");
                    alert.SetMessage("خطای نامشخص");
                    alert.SetPositiveButton(Resource.String.Ok, delegate { });
                    alert.Show();
                }
                finally
                {
                    pDialog.Dismiss();
                }
            };
            _saveButton.UpdateWidth();
            return view;
        }
    }
}