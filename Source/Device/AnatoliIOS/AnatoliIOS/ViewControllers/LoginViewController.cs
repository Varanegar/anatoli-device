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
                            AnatoliApp.GetInstance().Customer = await CustomerManager.ReadCustomerAsync();
                            AnatoliApp.GetInstance().User = await AnatoliUserManager.ReadUserInfoAsync();
                            AnatoliApp.GetInstance().RefreshMenu();
                            AnatoliApp.GetInstance().PushViewController(new ProductsViewController());
                        }
                    }
                    loadingOverlay.Hide();
                }
                catch (Exception ex)
                {
                    loadingOverlay.Hide();
                    if (ex.GetType() == typeof(TokenException))
                    {
                        var alert = UIAlertController.Create("خطا", "نام کاربری یا کلمه عبور اشتباه است", UIAlertControllerStyle.Alert);
                        alert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default, null));
                        PresentViewController(alert, true, null);
                    }
                }
            };
        }
    }
}