using System;
using Anatoli.App.Manager;
using Anatoli.App.Model.Product;
using AnatoliIOS.TableViewCells;
using UIKit;
using Foundation;
using ObjCRuntime;

namespace AnatoliIOS.TableViewSources
{
	public class ProductsTableViewSource : BaseTableViewSource<ProductManager,ProductModel>
	{
		public ProductsTableViewSource ()
		{
		}

		public override UIKit.UITableViewCell GetCellView (UIKit.UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell (ProductSummaryViewCell.Key) as ProductSummaryViewCell;
			if (cell == null) {
				var views = NSBundle.MainBundle.LoadNib (ProductSummaryViewCell.Key, tableView, null);
				cell = Runtime.GetNSObject (views.ValueAt (0)) as ProductSummaryViewCell;
				cell.BindCell (Items [indexPath.Row]);
			}
			cell.UpdateCell (Items [indexPath.Row]);
			return cell;
		}

		public override UITableViewRowAction[] EditActionsForRow (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			UITableViewRowAction favoritAction = null;
			UITableViewRowAction basketAction = null;
			if (Items [indexPath.Row].IsFavorit) {
				favoritAction = UITableViewRowAction.Create (UITableViewRowActionStyle.Destructive, "حذف از فهرست من", async delegate {
					tableView.Editing = false;
					var result = await ProductManager.RemoveFavoritAsync (Items [indexPath.Row]);
					if (result) {
						Items [indexPath.Row].favorit = 0;
					}
				});
			} else {
				favoritAction = UITableViewRowAction.Create (UITableViewRowActionStyle.Normal, "افزودن به فهرست من", async delegate {
					tableView.Editing = false;
					var result = await ProductManager.AddToFavorits (Items [indexPath.Row]);
					if (result) {
						Items [indexPath.Row].favorit = 1;
					}
				});
			}
			if (Items [indexPath.Row].ShoppingBasketCount > 0) {
				basketAction = UITableViewRowAction.Create (UITableViewRowActionStyle.Destructive, "حذف از سبد خرید", async delegate {
					tableView.Editing = false;
					var result = await ShoppingCardManager.RemoveProduct (Items [indexPath.Row], true);
					if (result) {
						tableView.ReloadData ();
						OnItemRemoved (tableView, indexPath);
					}
				});
			}
			if (basketAction == null)
				return new UITableViewRowAction[]{ favoritAction };
			else if (favoritAction == null)
				return new UITableViewRowAction[]{ basketAction };
			else
				return new UITableViewRowAction[]{ favoritAction, basketAction };
		}

		public override nfloat GetHeightForRow (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			return 70f;
		}
		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			tableView.DeselectRow (indexPath, true);
			ProductDetailViewController p = new ProductDetailViewController (Items [indexPath.Row]);
			AnatoliApp.GetInstance ().PresentViewController (p);
		}
	}
}

