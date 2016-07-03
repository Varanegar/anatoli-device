using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Anatoli.Framework.AnatoliBase;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Anatoli.App.Manager;
using AnatoliAndroid.Activities;
using Anatoli.Framework;

namespace AnatoliAndroid.Fragments
{
    public class RegisterFragment : DialogFragment
    {
        EditText _passwordEditText;
        EditText _telEditText;
        EditText _emailEditText;
        Button _registerButton;
        public override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Dialog.SetCanceledOnTouchOutside(false);
            Dialog.SetTitle("ثبت نام");
            var view = inflater.Inflate(Resource.Layout.RegisterLayout, container, false);
            _passwordEditText = view.FindViewById<EditText>(Resource.Id.passwordEditText);
            _telEditText = view.FindViewById<EditText>(Resource.Id.telEditText);
            _emailEditText = view.FindViewById<EditText>(Resource.Id.emailEditText);
            _registerButton = view.FindViewById<Button>(Resource.Id.registerButton);
            _registerButton.UpdateWidth();
            _registerButton.Click += _registerButton_Click;
            return view;
        }
        async void _registerButton_Click(object sender, EventArgs e)
        {
            AlertDialog.Builder alertDialog = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
            if (!AnatoliClient.GetInstance().WebClient.IsOnline())
            {
                alertDialog.SetMessage(Resource.String.PleaseConnectToInternet);
                alertDialog.SetTitle(Resource.String.Error);
                alertDialog.SetPositiveButton(Resource.String.Ok, (s, ev) => { });
                alertDialog.Show();
                return;
            }
            if (String.IsNullOrEmpty(_telEditText.Text) || String.IsNullOrEmpty(_passwordEditText.Text))
            {
                alertDialog.SetMessage(Resources.GetText(Resource.String.EneterUserNamePass));
                alertDialog.SetTitle(Resource.String.Error);
                alertDialog.SetPositiveButton(Resource.String.Ok, (s, ev) => { });
                alertDialog.Show();
                return;
            }
            if (_passwordEditText.Text.Length < 4)
            {
                alertDialog.SetMessage(Resources.GetText(Resource.String.PassLengthError));
                alertDialog.SetTitle(Resource.String.Error);
                alertDialog.SetPositiveButton(Resource.String.Ok, (s, ev) => { });
                alertDialog.Show();
                return;
            }
            _registerButton.Enabled = false;
            ProgressDialog dialog = new ProgressDialog(AnatoliApp.GetInstance().Activity);
            dialog.SetMessage(Resources.GetText(Resource.String.PleaseWait));
            dialog.Show();

            try
            {
                var result = await AnatoliUserManager.RegisterAsync(_passwordEditText.Text, _passwordEditText.Text, _telEditText.Text, _emailEditText.Text);
                dialog.Dismiss();
                if (result.IsValid) // register success
                {
                    alertDialog.SetMessage(Resources.GetText(Resource.String.SaveSuccess));
                    alertDialog.SetPositiveButton(Resource.String.Ok, (s, a) =>
                    {
                        // Show confirmation dialog
                        ConfirmDialog confirmDialog = new ConfirmDialog();
                        confirmDialog.UserName = _telEditText.Text;
                        confirmDialog.CodeConfirmed += async (sss, e2d) =>
                        {
                            // Login 
                            ProgressDialog pDialog = new ProgressDialog(AnatoliApp.GetInstance().Activity);
                            AlertDialog.Builder errDialog = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                            errDialog.SetPositiveButton(Resource.String.Ok, (s1, e1) => { });

                            pDialog.SetTitle(Resources.GetText(Resource.String.Login));
                            pDialog.SetMessage(Resources.GetText(Resource.String.PleaseWait));
                            pDialog.Show();
                            try
                            {
                                var userModel = await AnatoliUserManager.LoginAsync(_telEditText.Text, _passwordEditText.Text);
                                pDialog.Dismiss();
                                if (userModel.IsValid)
                                {
                                    await AnatoliApp.GetInstance().LoginAsync(userModel);
                                    try
                                    {
                                        Dismiss();
                                        AnatoliApp.GetInstance().PushFragment(new FirstFragment(), "first_fragment");
                                    }
                                    catch (Exception ex)
                                    {
                                        ex.SendTrace();
                                        if (ex.GetType() == typeof(TokenException))
                                        {
                                            errDialog.SetMessage(Resource.String.SaveFailed);
                                            errDialog.SetPositiveButton(Resource.String.Ok, (s2, e2) => { });
                                            errDialog.Show();
                                        }
                                    }
                                }
                                else
                                {
                                    errDialog.SetTitle(Resources.GetText(Resource.String.LoginFailed));
                                    errDialog.SetMessage(userModel.ModelStateString);
                                    errDialog.SetPositiveButton(Resource.String.Ok, (s2, e2) => { });
                                    errDialog.Show();
                                }
                            }
                            catch (Exception ex)
                            {
                                ex.SendTrace();
                                pDialog.Dismiss();
                                if (ex.GetType() == typeof(ServerUnreachableException))
                                {
                                    errDialog.SetMessage(Resources.GetText(Resource.String.ServerUnreachable));
                                    errDialog.SetPositiveButton(Resource.String.Ok, (s2, e2) => { });
                                    errDialog.Show();
                                }
                                else if (ex.GetType() == typeof(TokenException))
                                {
                                    errDialog.SetMessage(Resources.GetText(Resource.String.AuthenticationFailed));
                                    errDialog.SetPositiveButton(Resource.String.Ok, (s2, e2) => { });
                                    errDialog.Show();
                                }
                                else if (ex.GetType() == typeof(System.Threading.Tasks.TaskCanceledException))
                                {
                                    errDialog.SetMessage("خطا در برقراری ارتباط");
                                    errDialog.SetPositiveButton(Resource.String.Ok, (s2, e2) => { });
                                    errDialog.Show();
                                }
                            }

                        };
                        confirmDialog.ConfirmFailed += (msg) =>
                        {
                            AlertDialog.Builder alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                            alert.SetMessage(msg);
                            alert.SetTitle(Resource.String.Error);
                            alert.Show();
                        };
                        FragmentTransaction t = AnatoliApp.GetInstance().Activity.FragmentManager.BeginTransaction();
                        confirmDialog.Show(t, "confirm_dialog");
                        Dismiss();
                    });
                    alertDialog.Show();
                }
                else
                {
                    alertDialog.SetMessage(result.ModelStateString);
                    alertDialog.SetPositiveButton(Resource.String.Ok, (s, a) => { });
                    alertDialog.Show();
                }
            }
            catch (ServerUnreachableException)
            {
                alertDialog.SetMessage(Resource.String.ConnectionFailed);
                alertDialog.SetPositiveButton(Resource.String.Ok, (s, a) => { });
                alertDialog.Show();
            }
            catch (TokenException)
            {
                alertDialog.SetMessage(Resource.String.AuthenticationFailed);
                alertDialog.SetPositiveButton(Resource.String.Ok, (s, a) => { });
                alertDialog.Show();
            }
            catch (AnatoliWebClientException ex)
            {
                alertDialog.SetMessage(ex.MetaInfo.ModelStateString);
                alertDialog.SetPositiveButton(Resource.String.Ok, (s, a) => { });
                alertDialog.Show();
            }
            catch (Exception)
            {
                alertDialog.SetMessage("خطای نا مشخص!");
                alertDialog.SetPositiveButton(Resource.String.Ok, (s, a) => { });
                alertDialog.Show();
            }
            finally
            {
                dialog.Dismiss();
            }
        }
    }
}