using Anatoli.App.Manager;
using Anatoli.App.Model.Store;
using System;
using Anatoli.Framework.AnatoliBase;
using UIKit;
using System.Threading.Tasks;
using AnatoliIOS.Components;
using System.Collections.Generic;
using Foundation;
using ObjCRuntime;
using AnatoliIOS.TableViewCells;

namespace AnatoliIOS.ViewControllers
{
    public partial class OrderDetailViewController : BaseController
    {
        OrderModel _order;

        public OrderDetailViewController()
            : base("OrderDetailViewController", null)
        {
        }

        public OrderDetailViewController(OrderModel order)
            : this()
        {
            _order = order;
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }
        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            this.SetToolbarItems(AnatoliApp.GetInstance().CreateToolbarItems(), true);
        }
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Perform any additional setup after loading the view, typically from a nib.
            Title = "";
            EdgesForExtendedLayout = UIRectEdge.None;
            orderDateLabel.Text = "تاریخ سفارش : " + _order.order_date;
            orderPriceLabel.Text = "مبلغ : " + _order.order_price.ToCurrency() + " تومان";
            storeNameLabel.Text = "نام فروشگاه :‌ " + _order.store_name;
            storeStatusLabel.Text = "وضعیت سفارش : " + PurchaseOrderStatusHistoryViewModel.GetStatusName(_order.order_status);
            orderNoLabel.Text = "شماره سفارش : " + _order.order_id;
        }

        public async override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            if (AnatoliClient.GetInstance().WebClient.IsOnline())
            {

                await Task.Delay(1000);
                var loading = new LoadingOverlay(View.Frame, true);
                loading.Canceled += delegate
                {
                    loading.Hide();
                };
                try
                {
                    View.AddSubview(loading);
                    await PurchaseOrderManager.SyncOrderItemsAsync(AnatoliApp.GetInstance().Customer.UniqueId, _order);
                }
                catch (Exception)
                {

                }
                finally
                {
                    loading.Hide();
                }
            }
            else
            {
                var alert = UIAlertController.Create("", "برای اطلاع از آخرین وضعیت سفارش دستگاه خود را به اینترنت متصل نمایید.", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);
            }

            var items = await OrderItemsManager.GetItems(_order.order_id.ToString());
            OrderHistoryItemsTableViewSource source = new OrderHistoryItemsTableViewSource(items);
            itemsTableView.Source = source;
            itemsTableView.ReloadData();
        }
    }


    public class OrderHistoryItemsTableViewSource : UITableViewSource
    {
        List<OrderItemModel> _items;

        public OrderHistoryItemsTableViewSource(List<OrderItemModel> items)
        {
            if (items == null)
                items = new List<OrderItemModel>();
            _items = items;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _items.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(OrderHistoryItemSummaryViewCell.Key) as OrderHistoryItemSummaryViewCell;
            if (cell == null)
            {
                var views = NSBundle.MainBundle.LoadNib(OrderHistoryItemSummaryViewCell.Key, tableView, null);
                cell = Runtime.GetNSObject(views.ValueAt(0)) as OrderHistoryItemSummaryViewCell;
                cell.BindCell(_items[indexPath.Row]);
            }
            cell.UpdateCell(_items[indexPath.Row]);
            return cell;
        }
        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return 52f;
        }
    }
}