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
                catch (AnatoliWebClientException ex)
                {
                    var alert = UIAlertController.Create("Œÿ«", ex.MetaInfo.ModelStateString, UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("»«‘Â", UIAlertActionStyle.Default, null));
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