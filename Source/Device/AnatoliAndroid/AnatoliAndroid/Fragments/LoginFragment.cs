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
    public class LoginFragment : DialogFragment
    {
        EditText _userNameEditText;
        EditText _passwordEditText;
        Button _loginButton;
        Button _registerButton;
        TextView _fgTextView;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var view = inflater.Inflate(Resource.Layout.LoginLayout, container, false);

            _userNameEditText = view.FindViewById<EditText>(Resource.Id.userNameEditText);
            _passwordEditText = view.FindViewById<EditText>(Resource.Id.passwordEditText);
            _fgTextView = view.FindViewById<TextView>(Resource.Id.fgTextView);
            _registerButton = view.FindViewById<Button>(Resource.Id.registerButton);
            _registerButton.UpdateWidth();
            _registerButton.Click += _registerTextView_Click;
            _loginButton = view.FindViewById<Button>(Resource.Id.loginButton);
            _loginButton.UpdateWidth();
            _loginButton.Click += loginButton_Click;
            _fgTextView.Click += _fgTextView_Click;
            return view;
        }

        void _fgTextView_Click(object sender, EventArgs e)
        {
            Dismiss();
            ForgetPasswordDialog fDialog = new ForgetPasswordDialog();
            if (!string.IsNullOrEmpty(_userNameEditText.Text))
            {
                fDialog.UserName = _userNameEditText.Text;
            }
            var t = AnatoliApp.GetInstance().Activity.FragmentManager.BeginTransaction();
            fDialog.Show(t, "forgetpass_dialog");
        }

        void _registerTextView_Click(object sender, EventArgs e)
        {
            Dismiss();
            var transaction = Activity.FragmentManager.BeginTransaction();
            var regFragment = new RegisterFragment();
            regFragment.Show(transaction, "register_fragment");
        }

        async void loginButton_Click(object sender, EventArgs e)
        {

            if (String.IsNullOrEmpty(_userNameEditText.Text) || String.IsNullOrEmpty(_passwordEditText.Text))
            {
                AlertDialog.Builder errDialog = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                errDialog.SetMessage(Resources.GetText(Resource.String.EneterUserNamePass));
                errDialog.SetPositiveButton(Resource.String.Ok, delegate { });
                errDialog.Show();
                return;
            }
            if (!AnatoliClient.GetInstance().WebClient.IsOnline())
            {
                AlertDialog.Builder errDialog = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                errDialog.SetTitle(Resources.GetText(Resource.String.NetworkAccessFailed));
                errDialog.SetMessage(Resources.GetText(Resource.String.PleaseConnectToInternet));
                errDialog.SetPositiveButton(Resource.String.Ok, delegate
                {
                    Intent intent = new Intent(Android.Provider.Settings.ActionSettings);
                    AnatoliApp.GetInstance().Activity.StartActivity(intent);
                });
                errDialog.SetNegativeButton(Resource.String.Cancel, delegate { });
                errDialog.Show();
                return;
            }
            _loginButton.Enabled = false;
            ProgressDialog pDialog = new ProgressDialog(AnatoliApp.GetInstance().Activity);
            pDialog.SetCanceledOnTouchOutside(false);
            try
            {
                pDialog.SetTitle(Resources.GetText(Resource.String.Login));
                pDialog.SetMessage(Resources.GetText(Resource.String.PleaseWait));
                pDialog.Show();
                var userModel = await AnatoliUserManager.LoginAsync(_userNameEditText.Text, _passwordEditText.Text);
                pDialog.Dismiss();
                if (userModel != null && userModel.IsValid)
                {
                    await AnatoliApp.GetInstance().LoginAsync(userModel);
                    Dismiss();
                    OnLoginSuccess();
                }
                else
                {
                    AlertDialog.Builder errDialog = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                    errDialog.SetTitle(Resources.GetText(Resource.String.LoginFailed));
                    errDialog.SetMessage(userModel.ModelStateString);
                    errDialog.SetPositiveButton(Resource.String.Ok, delegate { });
                    errDialog.Show();
                }
            }
            catch (Exception ex)
            {
                pDialog.Dismiss();
                if (ex.GetType() == typeof(ServerUnreachableException))
                {
                    AlertDialog.Builder errDialog = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                    errDialog.SetMessage(Resources.GetText(Resource.String.ServerUnreachable));
                    errDialog.SetPositiveButton(Resource.String.Ok, delegate { });
                    errDialog.Show();
                }
                else if (ex.GetType() == typeof(TokenException))
                {
                    AlertDialog.Builder errDialog = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                    errDialog.SetMessage(Resources.GetText(Resource.String.AuthenticationFailed));
                    errDialog.SetPositiveButton(Resource.String.Ok, delegate { });
                    errDialog.Show();
                }
                else if (ex.GetType() == typeof(UnConfirmedUserException))
                {
                    AlertDialog.Builder alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                    alert.SetTitle(Resource.String.Error);
                    alert.SetMessage("شماره شما هنوز تایید نشده است. کد فعال سازی ارسال شود؟");
                    alert.SetPositiveButton(Resource.String.Yes, async delegate
                    {
                        ConfirmDialog confirmDialog = new ConfirmDialog();
                        confirmDialog.UserName = _userNameEditText.Text;
                        confirmDialog.CodeConfirmed += async delegate
                        {
                            AlertDialog.Builder errDialog = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                            errDialog.SetPositiveButton(Resource.String.Ok, delegate { });
                            pDialog.SetTitle(Resources.GetText(Resource.String.Login));
                            pDialog.SetMessage(Resources.GetText(Resource.String.PleaseWait));
                            pDialog.Show();
                            try
                            {
                                var userModel = await AnatoliUserManager.LoginAsync(_userNameEditText.Text, _passwordEditText.Text);
                                pDialog.Dismiss();
                                if (userModel != null && userModel.IsValid)
                                {
                                    try
                                    {
                                        await AnatoliApp.GetInstance().LoginAsync(userModel);
                                        Dismiss();
                                        OnLoginSuccess();
                                    }
                                    catch (Exception ex2)
                                    {
                                        ex2.SendTrace();
                                        if (ex2.GetType() == typeof(TokenException))
                                        {
                                            AlertDialog.Builder errDialog2 = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                                            errDialog2.SetMessage(Resource.String.SaveFailed);
                                            errDialog2.SetPositiveButton(Resource.String.Ok, delegate { });
                                            errDialog2.Show();
                                        }
                                        else
                                        {
                                            AlertDialog.Builder errDialog2 = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                                            errDialog2.SetMessage(Resource.String.ErrorOccured);
                                            errDialog2.SetPositiveButton(Resource.String.Ok, delegate { });
                                            errDialog2.Show();
                                        }

                                    }
                                }
                                else
                                {
                                    AlertDialog.Builder errDialog3 = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                                    errDialog3.SetTitle(Resources.GetText(Resource.String.LoginFailed));
                                    errDialog3.SetMessage(userModel.ModelStateString);
                                    errDialog3.SetPositiveButton(Resource.String.Ok, delegate { });
                                    errDialog3.Show();
                                }
                            }
                            catch (Exception ex2)
                            {
                                pDialog.Dismiss();
                                if (ex2.GetType() == typeof(ServerUnreachableException))
                                {
                                    AlertDialog.Builder errDialog4 = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                                    errDialog4.SetMessage(Resources.GetText(Resource.String.ServerUnreachable));
                                    errDialog4.SetPositiveButton(Resource.String.Ok, delegate { });
                                    errDialog4.Show();
                                }
                                else if (ex2.GetType() == typeof(TokenException))
                                {
                                    AlertDialog.Builder errDialog4 = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                                    errDialog4.SetMessage(Resources.GetText(Resource.String.AuthenticationFailed));
                                    errDialog4.SetPositiveButton(Resource.String.Ok, delegate { });
                                    errDialog4.Show();
                                }
                                else if (ex.GetType() == typeof(System.Threading.Tasks.TaskCanceledException))
                                {
                                    AlertDialog.Builder errDialog4 = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                                    errDialog4.SetMessage("خطا در برقراری ارتباط");
                                    errDialog4.SetPositiveButton(Resource.String.Ok, delegate { });
                                    errDialog4.Show();
                                }
                                else
                                {
                                    AlertDialog.Builder errDialog4 = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                                    errDialog4.SetMessage(Resource.String.ErrorOccured);
                                    errDialog4.SetPositiveButton(Resource.String.Ok, delegate { });
                                    errDialog4.Show();
                                    ex2.SendTrace();
                                }
                            }

                        };
                        confirmDialog.ConfirmFailed += (msg) =>
                        {
                            alert.SetMessage(msg);
                            alert.SetTitle(Resource.String.Error);
                            alert.Show();
                        };
                        FragmentTransaction t = AnatoliApp.GetInstance().Activity.FragmentManager.BeginTransaction();
                        confirmDialog.Show(t, "confirm_dialog");
                        await AnatoliUserManager.RequestConfirmCode(_userNameEditText.Text);
                    });
                    alert.SetNegativeButton(Resource.String.Cancel, delegate { });
                    alert.Show();
                }
                else
                {
                    AlertDialog.Builder errDialog = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                    errDialog.SetPositiveButton(Resource.String.Ok, delegate { });
                    errDialog.SetTitle(Resource.String.Error);
                    errDialog.SetMessage(Resource.String.ErrorOccured);
                    errDialog.Show();
                    ex.SendTrace();
                }
            }
            _loginButton.Enabled = true;
        }

        void OnLoginSuccess()
        {
            if (LoginSuceeded != null)
            {
                LoginSuceeded.Invoke();
            }
        }

        public event LoginSuccessEventHandler LoginSuceeded;
        public delegate void LoginSuccessEventHandler();
    }
}