using Anatoli.App.Manager;
using Anatoli.Framework.AnatoliBase;
using AnatoliIOS.Components;
using System;
using UIKit;

namespace AnatoliIOS.ViewControllers
{
    public partial class RegisterViewController : BaseController
    {
        public RegisterViewController()
            : base("RegisterViewController", null)
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

            // Perform any additional setup after loading the view, typically from a nib.
            Title = "ثبت نام";
            EdgesForExtendedLayout = UIRectEdge.None;
            phoneTextField.ShouldReturn += delegate
            {
                emailTextField.BecomeFirstResponder();
                return true;
            };
            emailTextField.ShouldReturn += delegate
            {
                passwordTextField.BecomeFirstResponder();
                return true;
            };
            passwordTextField.ShouldReturn += delegate
            {
                passwordTextField.ResignFirstResponder();
                return true;
            };
            registerButton.TouchUpInside += async delegate
            {
                if (String.IsNullOrEmpty(phoneTextField.Text))
                {
                    return;
                }

                LoadingOverlay loading = new LoadingOverlay(View.Bounds);
                try
                {
                    View.AddSubview(loading);
                    var result = await AnatoliUserManager.RegisterAsync(passwordTextField.Text, passwordTextField.Text, phoneTextField.Text, emailTextField.Text);
                    if (result != null && result.IsValid)
                    {
                        loading.Hidden = true;
                        AnatoliApp.GetInstance().PushViewController(new ConfirmRegisterationViewController(phoneTextField.Text, passwordTextField.Text));
                    }

                }
                catch (ConnectionFailedException)
                {

                    var alert = UIAlertController.Create("خطا", "خطا در برقراری ارتباط", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
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
                finally
                {
                    loading.Hidden = true;
                }
            };
        }
    }
}