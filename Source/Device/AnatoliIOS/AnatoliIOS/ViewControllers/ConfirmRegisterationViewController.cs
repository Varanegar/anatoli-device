using Anatoli.App.Manager;
using Anatoli.Framework.AnatoliBase;
using AnatoliIOS.Components;
using System;
using UIKit;

namespace AnatoliIOS.ViewControllers
{
    public partial class ConfirmRegisterationViewController : UIViewController
    {
        private string _username;
        private string _password;

        public ConfirmRegisterationViewController()
            : base("ConfirmRegisterationViewController", null)
        {
        }

        public ConfirmRegisterationViewController(string username, string password)
        {
            // TODO: Complete member initialization
            _username = username;
            _password = password;
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            codeTextField.ShouldReturn += delegate
            {
                codeTextField.ResignFirstResponder();
                return true;
            };
            codeButton.TouchUpInside += delegate
            {
                ResignFirstResponder();
                var alert = UIAlertController.Create("", "کد رمز دوباره ارسال شود؟", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("بله", UIAlertActionStyle.Default,
                    async delegate
                    {
                        LoadingOverlay loading = new LoadingOverlay(View.Bounds);
                        View.AddSubview(loading);
                        try
                        {
                            var codeResult = await AnatoliUserManager.RequestConfirmCode(_username);
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
                        catch (Exception)
                        {
                            //todo : handle exception
                        }
                        finally
                        {
                            loading.Hidden = true;
                        }
                    }
                    ));
                alert.AddAction(UIAlertAction.Create("بی خیال", UIAlertActionStyle.Cancel, null));
                PresentViewController(alert, true, null);
            };
            sendButton.TouchUpInside += async delegate
            {
                ResignFirstResponder();
                if (!String.IsNullOrEmpty(codeTextField.Text))
                {
                    LoadingOverlay loading = new LoadingOverlay(View.Bounds);
                    View.AddSubview(loading);
                    try
                    {
                        var result = await AnatoliUserManager.SendConfirmCode(_username, codeTextField.Text);
                        if (result != null && result.IsValid)
                        {
                            var alert = UIAlertController.Create("", "حساب کاربری شما با موفقیت فعال شد", UIAlertControllerStyle.Alert);
                            alert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default, async delegate
                            {
                                await AnatoliApp.GetInstance().LoginAsync();
                                AnatoliApp.GetInstance().PushViewController(new ProductsViewController());
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
                        var alert = UIAlertController.Create("", "درخواست شما با خطا مواجه شد", UIAlertControllerStyle.Alert);
                        alert.AddAction(UIAlertAction.Create("خب", UIAlertActionStyle.Default, null));
                        PresentViewController(alert, true, null);
                    }
                    finally
                    {
                        loading.Hidden = true;
                    }
                }
            };
        }
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
        }
    }
}