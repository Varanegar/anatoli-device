using System;

using UIKit;
using CoreGraphics;
using Anatoli.App.Model.Store;
using Anatoli.Framework.AnatoliBase;
using AnatoliIOS.TableViewCells;
using Foundation;
using ObjCRuntime;
using System.Collections.Generic;

namespace AnatoliIOS.ViewControllers
{
    public partial class ProformaViewController : BaseController
    {
        PurchaseOrderViewModel _order;

        public ProformaViewController()
            : base("ProformaViewController", null)
        {
        }

        public ProformaViewController(PurchaseOrderViewModel order)
            : this()
        {
            _order = order;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
            Title = "پیش فاکتور";
            var headerView = ProformHeader.Create();
            header.AddSubview(headerView);

            var footerView = FooterView.Create();
            footer.AddSubview(footerView);
            cancelButton.TouchUpInside += (object sender, EventArgs e) =>
            {
                DismissViewController(true, null);
            };
            orderDate.Text = _order.OrderDate.ToString();
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
            table.Source = new OrderItemsTableViewSource(_order.LineItems);
            //table.TableFooterView.Bounds = new CoreGraphics.CGRect (0, -10, View.Frame.Width, table.TableFooterView.Bounds.Height);
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
        public OrderItemsTableViewSource(List<PurchaseOrderLineItemViewModel> items)
        {
            _items = items;
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
                    cell.Update(_items[indexPath.Row],tableView, indexPath);
                }
            }
            return cell;
        }
    }
}


