using System;
using Anatoli.App.Manager;
using Anatoli.App.Model.Store;
using AnatoliIOS.TableViewCells;
using AnatoliIOS.ViewControllers;
using UIKit;

namespace AnatoliIOS.TableViewSources
{
	public class StoresTableViewSource : BaseTableViewSource<StoreManager, StoreDataModel>
	{
		public StoresTableViewSource ()
		{
		}

		public override UIKit.UITableViewCell GetCellView (UIKit.UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell (StoreSummaryTableViewCell.Key) as StoreSummaryTableViewCell;
			cell.UpdateCell (Items [indexPath.Row]);
			return cell;
		}

		public async override void RowSelected (UIKit.UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			var item = Items [indexPath.Row];
			if (item != null) {
				if (!item.support_app_order) {
					var alert = new UIAlertView ("", "این فروشگاه امکان فروش اینترنتی را ندارد", null, "باشه");
					alert.Show ();
					return;
				}
				if ((await ShoppingCardManager.GetInfoAsync ()).items_count > 0) {
					var alert = UIAlertController.Create ("اخطار", ".با تغییر فروشگاه سبد خرید شما خالی میشود. ادامه می دهید؟", UIAlertControllerStyle.Alert);
					alert.AddAction (UIAlertAction.Create ("بی خیال", UIAlertActionStyle.Cancel, null));
					alert.AddAction (UIAlertAction.Create ("باشه", UIAlertActionStyle.Default,
						async delegate {
							var result = await StoreManager.SelectAsync (item);
							if (result) {
								AnatoliApp.GetInstance ().DefaultStore = item;
								AnatoliApp.GetInstance ().ReplaceViewController (new FirstPageViewController ());
							}
						}));
					AnatoliApp.GetInstance ().PresentViewController (alert);
				} else {
					var result = await StoreManager.SelectAsync (item);
					if (result) {
						AnatoliApp.GetInstance ().DefaultStore = item;
						AnatoliApp.GetInstance ().ReplaceViewController (new FirstPageViewController ());
					}
				}
                
			}

		}

		public override nfloat GetHeightForRow (UIKit.UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			return 70f;
		}
	}
}

