using System;

using UIKit;
using CoreGraphics;
using Anatoli.App.Model.Store;
using Anatoli.Framework.AnatoliBase;
using AnatoliIOS.TableViewCells;
using Foundation;
using ObjCRuntime;
using System.Collections.Generic;
using Anatoli.App.Manager;
using System.Globalization;

namespace AnatoliIOS.ViewControllers
{
    public partial class ProformaViewController : BaseController
    {
        PurchaseOrderViewModel _order;
        DeliveryTimeModel _deliveryTime;
        DeliveryTypeModel _deliveryType;
        public ProformaViewController()
            : base("ProformaViewController", null)
        {
        }

        public ProformaViewController(PurchaseOrderViewModel order, DeliveryTimeModel deliveryTime, DeliveryTypeModel deliveryType)
            : this()
        {
            _order = order;
            _deliveryTime = deliveryTime;
            _deliveryType = deliveryType;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            this.SetToolbarItems(AnatoliApp.GetInstance().CreateToolbarItems(), true);
        }
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
            Title = "پیش فاکتور";
            EdgesForExtendedLayout = UIRectEdge.None;
			titleView.BackgroundColor = UIColor.Clear.FromHex(0x085e7d);
			okButton.SetStyle (ButtonColor.Green);
            var headerView = ProformHeader.Create();
			var hframe = headerView.Frame;
			hframe.Width = header.Frame.Width;
			headerView.Frame = hframe;
            header.AddSubview(headerView);

            var footerView = FooterView.Create();
			var fframe = footerView.Frame;
			fframe.Width = footer.Frame.Width;
			footerView.Frame = fframe;
            footer.AddSubview(footerView);
            cancelButton.TouchUpInside += (object sender, EventArgs e) =>
            {
                DismissViewController(true, null);
            };
            var pDate = new PersianCalendar();
            orderDate.Text = pDate.GetYear(_order.OrderDate.Value) + "/" + pDate.GetMonth(_order.OrderDate.Value) + "/" + pDate.GetDayOfMonth(_order.OrderDate.Value);
            orderSum.Text = _order.NetAmount.ToCurrency() + " تومان";
            orderAddress.Text = AnatoliApp.GetInstance().Customer.MainStreet;
            footerView.Price = _order.Amount.ToCurrency();
            footerView.Discount = _order.DiscountAmount.ToCurrency();
            decimal count = 0;
            foreach (var item in _order.LineItems)
            {
                count += item.Qty;
            }
            footerView.Count = count.ToString("N0");
            footerView.Tax = (_order.ChargeAmount + _order.TaxAmount).ToCurrency();
            table.Source = new OrderItemsTableViewSource(_order.LineItems, _order);
            okButton.TouchUpInside += async delegate
            {
                try
                {
                    var result = await ShoppingCardManager.Checkout(AnatoliApp.GetInstance().Customer,
                    AnatoliApp.GetInstance().User.Id,
                    AnatoliApp.GetInstance().DefaultStore.store_id,
                    _deliveryType.id,
                    _deliveryTime);
                    if (result != null && result.IsValid)
                    {
						await ShoppingCardManager.ClearAsync();
                        var alert = UIAlertController.Create("", "سفارش شما ثبت شد", UIAlertControllerStyle.Alert);
                        alert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default, delegate
                        {
                            AnatoliApp.GetInstance().PushViewController(new FirstPageViewController());
                        }));
                        PresentViewController(alert, true, null);
                    }
                    else
                    {
                        var alert = UIAlertController.Create("خطا", "خطا در ارسال سفارش", UIAlertControllerStyle.Alert);
                        alert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default, delegate
                        {
                            DismissViewController(true, null);
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
                    var alert = UIAlertController.Create("خطا", "خطا در ارسال سفارش", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default, delegate
                    {
                        DismissViewController(true, null);
                    }));
                    PresentViewController(alert, true, null);
                }
            };
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
    class OrderItemsTableViewSource : UITableViewSource
    {
        List<PurchaseOrderLineItemViewModel> _items;
        PurchaseOrderViewModel _order;
        public OrderItemsTableViewSource(List<PurchaseOrderLineItemViewModel> items, PurchaseOrderViewModel order)
        {
            _items = items;
            _order = order;
        }
        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _items.Count;
        }
        public override UITableViewCell GetCell(UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            OrderItemTableViewCell cell;
            cell = tableView.DequeueReusableCell(OrderItemTableViewCell.Key) as OrderItemTableViewCell;
            if (cell == null)
            {
                var views = NSBundle.MainBundle.LoadNib(OrderItemTableViewCell.Key, tableView, null);
                cell = Runtime.GetNSObject(views.ValueAt(0)) as OrderItemTableViewCell;
            }
            if (_items != null)
            {
                if (_items.Count > indexPath.Row)
                {
                    cell.Update(_items[indexPath.Row], _order, tableView, indexPath);
                }
            }
            return cell;
        }
		public override nfloat GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			return 30f;
		}
    }
}


