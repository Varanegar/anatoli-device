using System;

using UIKit;
using AnatoliIOS.TableViewSources;
using Anatoli.App.Manager;

namespace AnatoliIOS.ViewControllers
{
	public partial class OrdersStatusViewController : BaseController
	{
		public OrdersStatusViewController () : base ("OrdersStatusViewController", null)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			Title = "سفارشات قبلی";
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}
		public async override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			var source = new OrderStatusTableViewSource ();
			source.SetDataQuery(OrderManager.GetOrderQueryString());
			await source.RefreshAsync ();
			ordersTableView.Source = source;
			ordersTableView.ReloadData ();
		}
	}
}


