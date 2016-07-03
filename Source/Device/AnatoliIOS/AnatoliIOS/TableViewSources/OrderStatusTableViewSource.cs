using System;
using Anatoli.App.Manager;
using Anatoli.App.Model.Store;
using AnatoliIOS.TableViewCells;
using Foundation;
using ObjCRuntime;
using UIKit;
using AnatoliIOS.ViewControllers;

namespace AnatoliIOS.TableViewSources
{
	public class OrderStatusTableViewSource : BaseTableViewSource<PurchaseOrderManager,OrderModel>
	{
		public OrderStatusTableViewSource ()
		{
		}
		public override UIKit.UITableViewCell GetCellView (UIKit.UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell (OrderStatusTableViewCell.Key) as OrderStatusTableViewCell;
			if (cell == null) {
				var views = NSBundle.MainBundle.LoadNib (OrderStatusTableViewCell.Key, tableView, null);
				cell = Runtime.GetNSObject (views.ValueAt (0)) as OrderStatusTableViewCell;
			}
			cell.Update (Items [indexPath.Row]);
			return cell;
		}
		public override nfloat GetHeightForRow (UIKit.UITableView tableView, NSIndexPath indexPath)
		{
			return 104;
		}
        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            tableView.DeselectRow(indexPath, true);
            OrderDetailViewController p = new OrderDetailViewController(Items[indexPath.Row]);
            AnatoliApp.GetInstance().PushViewController(p);
        }
    }
}

