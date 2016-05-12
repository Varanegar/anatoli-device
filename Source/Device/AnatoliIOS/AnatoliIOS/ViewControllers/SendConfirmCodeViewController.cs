using Anatoli.App.Manager;
using Anatoli.Framework.AnatoliBase;
using AnatoliIOS.Components;
using System;
using UIKit;

namespace AnatoliIOS.ViewControllers
{
    public partial class SendConfirmCodeViewController : UIViewController
    {
        private string _username;

        public SendConfirmCodeViewController()
            : base("SendConfirmCodeViewController", null)
        {
        }

        public SendConfirmCodeViewController(string username)
        {
            // TODO: Complete member initialization
            _username = username;
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            EdgesForExtendedLayout = UIRectEdge.None;
            codeTextField.ShouldReturn += delegate
            {
                codeTextField.ResignFirstResponder();
                return true;
            };
            passwordEditText.ShouldReturn += delegate
            {
                passwordEditText.ResignFirstResponder();
                return true;
            };
            password2EditText.ShouldReturn += delegate
            {
                password2EditText.ResignFirstResponder();
                return true;
            };
            sendButton.TouchUpInside += async delegate
            {
                ResignFirstResponder();
                if (string.IsNullOrEmpty(passwordEditText.Text))
                {
                    var alert = UIAlertController.Create("خطا", "لطفا کلمه عبور را وارد نمایید", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                    return;
                }
                if (string.IsNullOrEmpty(password2EditText.Text))
                {
                    var alert = UIAlertController.Create("خطا", "لطفا تکرار کلمه عبور را وارد نمایید", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                    return;
                }
                if (!passwordEditText.Text.Equals(password2EditText.Text))
                {
                    var alert = UIAlertController.Create("خطا", "کلمه عبور و تکرار آن یکسان نیستند", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
                if (!string.IsNullOrEmpty(codeTextField.Text))
                {
                    LoadingOverlay loading = new LoadingOverlay(View.Bounds);
                    View.AddSubview(loading);
                    try
                    {
                        var result = await AnatoliUserManager.ResetPasswordByCode(_username, passwordEditText.Text, codeTextField.Text);
                        if (result != null && result.IsValid)
                        {
                            var alert = UIAlertController.Create("", "کلمه عبور با موفقیت تغییر یافت", UIAlertControllerStyle.Alert);
                            alert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default, delegate
                            {
                                loading.Hidden = true;
                                AnatoliApp.GetInstance().ReplaceViewController(new FirstPageViewController());
                            }));
                            PresentViewController(alert, true, null);
                        }
                    }
                    catch (ServerUnreachableException)
                    {
                        var connectionalert = UIAlertController.Create("خطا", "خطا در برقرای ارتباط", UIAlertControllerStyle.Alert);
                        connectionalert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default, null));
                        PresentViewController(connectionalert, true, null);
                    }
                    catch (NoInternetAccessException)
                    {
                        var connectionalert = UIAlertController.Create("خطا", "لطفا دستگاه خود را به اینترنت متصل نمایید", UIAlertControllerStyle.Alert);
                        connectionalert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default, null));
                        PresentViewController(connectionalert, true, null);
                    }
                    catch (AnatoliWebClientException ex)
                    {
                        if (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
                        {
                            var alert = UIAlertController.Create("خطا", ex.MetaInfo.ModelStateString, UIAlertControllerStyle.Alert);
                            alert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default, null));
                            PresentViewController(alert, true, null);
                        }
                    }
                    catch (Exception)
                    {
                        var alert = UIAlertController.Create("خطا", "درخواست شما با خطا مواجه شد!", UIAlertControllerStyle.Alert);
                        alert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default, null));
                        PresentViewController(alert, true, null);
                    }
                    finally
                    {
                        loading.Hidden = true;
                    }
                }
                else
                {
                    var alert = UIAlertController.Create("خطا", "لطفا کد رمز دریافت شده را وارد نمایید", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
            };
        }
    }
}