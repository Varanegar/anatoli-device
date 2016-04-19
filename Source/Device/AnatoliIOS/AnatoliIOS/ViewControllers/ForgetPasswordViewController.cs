using Anatoli.App.Manager;
using Anatoli.Framework.AnatoliBase;
using AnatoliIOS.Components;
using System;
using UIKit;

namespace AnatoliIOS.ViewControllers
{
    public partial class ForgetPasswordViewController : UIViewController
    {
        public ForgetPasswordViewController()
            : base("ForgetPasswordViewController", null)
        {
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
            phoneTextField.ShouldReturn += delegate
            {
                phoneTextField.ResignFirstResponder();
                return true;
            };
            passConformTextField.ShouldReturn += delegate
            {
                passConformTextField.ResignFirstResponder();
                return true;
            };
            passwordTextField.ShouldReturn += delegate
            {
                passwordTextField.ResignFirstResponder();
                return true;
            };
            sendButton.TouchUpInside += async delegate
            {
                LoadingOverlay loading = new LoadingOverlay(View.Bounds);

                try
                {
                    View.AddSubview(loading);
                    var result = await AnatoliUserManager.ResetPassword(phoneTextField.Text, passwordTextField.Text);
                    if (result != null && result.IsValid)
                    {
                        loading.Hidden = true;
                        AnatoliApp.GetInstance().PushViewController(new SendConfirmCodeViewController(phoneTextField.Text));
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
                    var alert = UIAlertController.Create("", "درخواست شما با خطا مواجه شد", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("خب", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
                finally
                {
                    loading.Hidden = true;
                }
            };
        }
    }
}