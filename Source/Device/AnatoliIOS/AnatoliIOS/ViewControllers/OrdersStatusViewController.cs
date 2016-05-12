using System;

using UIKit;
using AnatoliIOS.TableViewSources;
using Anatoli.App.Manager;
using AnatoliIOS.Components;
using Anatoli.Framework.AnatoliBase;

namespace AnatoliIOS.ViewControllers
{
    public partial class OrdersStatusViewController : BaseController
    {
        public OrdersStatusViewController() : base("OrdersStatusViewController", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            EdgesForExtendedLayout = UIRectEdge.None;
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
        public async override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            Title = "سفارشات قبلی";
            if (AnatoliClient.GetInstance().WebClient.IsOnline())
            {
                LoadingOverlay loading = new LoadingOverlay(View.Bounds);
                View.AddSubview(loading);
                try
                {
                    await OrderManager.SyncOrdersAsync(AnatoliApp.GetInstance().Customer.UniqueId);
                }
                catch (AnatoliWebClientException ex)
                {
                    var alert = new UIAlertView("", ex.MetaInfo.ModelStateString, null, "باشه");
                    alert.Show();
                }
                catch (Exception)
                {
                    var alert = new UIAlertView("", "خطا در اجرای درخواست شما", null, "باشه");
                    alert.Show();
                }
                finally
                {
                    loading.Hide();
                }
            }
            else
            {
                var alert = new UIAlertView("", "برای دریافت آخرین اطلاعات سفارشات خود باید به اینترنت متصل باشید", null, "باشه");
                alert.Show();
            }

            var source = new OrderStatusTableViewSource();
            source.SetDataQuery(OrderManager.GetOrderQueryString());
            await source.RefreshAsync();
            ordersTableView.Source = source;
            ordersTableView.ReloadData();
        }
    }
}


