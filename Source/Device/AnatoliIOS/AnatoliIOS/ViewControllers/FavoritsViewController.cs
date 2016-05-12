using System;

using UIKit;
using AnatoliIOS.ViewControllers;
using AnatoliIOS.TableViewSources;
using Anatoli.App.Manager;

namespace AnatoliIOS.ViewControllers
{
	public partial class FavoritsViewController : BaseController
	{
		ProductsTableViewSource _tableViewSource;
		public FavoritsViewController () : base ("FavoritsViewController", null)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
            // Perform any additional setup after loading the view, typically from a nib.
        }
		public async override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
            Title = "لیست من";
            EdgesForExtendedLayout = UIRectEdge.None;
            _tableViewSource = new ProductsTableViewSource ();
			_tableViewSource.SetDataQuery (ProductManager.GetFavoritsQueryString (AnatoliApp.GetInstance().DefaultStore.store_id));
			await _tableViewSource.RefreshAsync ();
			tableView.Source = _tableViewSource;
			tableView.ReloadData ();
		}
		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}
	}
}


