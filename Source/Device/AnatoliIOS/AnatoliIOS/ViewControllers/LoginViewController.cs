using Anatoli.App.Manager;
using System;
using UIKit;
using Anatoli.Framework.AnatoliBase;
using AnatoliIOS.Components;
namespace AnatoliIOS.ViewControllers
{
    public partial class LoginViewController : BaseController
    {
        public LoginViewController()
            : base("LoginViewController", null)
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
            Title = "ورود";

            EdgesForExtendedLayout = UIRectEdge.None;
            userNameTextField.ShouldReturn += (textField) =>
            {
                textField.ResignFirstResponder();
                return true;
            };
            passwordTextField.ShouldReturn += (textField) =>
            {
                textField.ResignFirstResponder();
                return true;
            };
            
            registerButton.TouchUpInside += delegate
            {
                AnatoliApp.GetInstance().PushViewController(new RegisterViewController());
            };
            forgotPasswordButton.TouchUpInside += delegate
            {
                AnatoliApp.GetInstance().PushViewController(new ForgetPasswordViewController());
            };
            loginButton.TouchUpInside += async delegate
            {
                ResignFirstResponder();
                if (String.IsNullOrEmpty(userNameTextField.Text) || String.IsNullOrEmpty(passwordTextField.Text))
                {
                    var alert = UIAlertController.Create("خطا", "ورود نام کاربری و کلمه عبور اجباریست!", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("خب", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
                LoadingOverlay loadingOverlay;
                var bounds = UIScreen.MainScreen.Bounds;
                loadingOverlay = new LoadingOverlay(bounds);
                View.Add(loadingOverlay);
                try
                {

                    var result = await AnatoliUserManager.LoginAsync(userNameTextField.Text, passwordTextField.Text);
                    if (result != null)
                    {
                        if (result.IsValid)
                        {
                            await AnatoliApp.GetInstance().LoginAsync();
                            AnatoliApp.GetInstance().PushViewController(new ProductsViewController());
                        }
                    }
                    loadingOverlay.Hide();
                }
                catch (AnatoliWebClientException ex)
                {
                    var alert = UIAlertController.Create("خطا", ex.MetaInfo.ModelStateString, UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
                catch (TokenException)
                {
                    loadingOverlay.Hide();
                    var alert = UIAlertController.Create("خطا", "نام کاربری یا کلمه عبور اشتباه است", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }

                catch (UnConfirmedUserException)
                {
                    var alert = UIAlertController.Create("خطا", "ثبت نام شما هنوز کامل نشده است!", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("بی خیال", UIAlertActionStyle.Cancel, null));
                    alert.AddAction(UIAlertAction.Create("تایید حساب کاربری", UIAlertActionStyle.Default, async delegate
                    {
                        try
                        {
                            var codeResult = await AnatoliUserManager.RequestConfirmCode(userNameTextField.Text);
                            AnatoliApp.GetInstance().PushViewController(new ConfirmRegisterationViewController(userNameTextField.Text, passwordTextField.Text));
                        }
                        catch (Exception)
                        {

                        }
                    }));
                    PresentViewController(alert, true, null);
                }
                catch (Exception)
                {
                    var alert = UIAlertController.Create("خطا", "درخواست شما با خطا مواجه شد!", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Cancel, null));
                    PresentViewController(alert, true, null);
                }
                finally
                {
                    loadingOverlay.Hidden = true;
                }
            };
        }
    }
}