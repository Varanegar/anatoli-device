using System;

using UIKit;
using CoreGraphics;
using Anatoli.App.Model.Store;
using Anatoli.Framework.AnatoliBase;

namespace AnatoliIOS.ViewControllers
{
	public partial class ProformaViewController : BaseController
	{
		PurchaseOrderViewModel _order;

		public ProformaViewController () : base ("ProformaViewController", null)
		{
		}

		public ProformaViewController (PurchaseOrderViewModel order) : this ()
		{
			_order = order;
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			// Perform any additional setup after loading the view, typically from a nib.
			Title = "پیش فاکتور";
			var headerView = ProformHeader.Create ();
			header.AddSubview (headerView);

			var footerView = FooterView.Create ();
			footer.AddSubview (footerView);
			cancelButton.TouchUpInside += (object sender, EventArgs e) => {
				DismissViewController (true, null);
			};
			orderDate.Text = _order.OrderDate.ToString ();
			orderSum.Text = _order.NetAmount.ToCurrency () + " تومان";
			orderAddress.Text = AnatoliApp.GetInstance ().Customer.MainStreet;
			footerView.Price = _order.Amount.ToCurrency ();
			footerView.Discount = _order.DiscountAmount.ToCurrency ();
			decimal count = 0;
			foreach (var item in _order.LineItems) {
				count += item.Qty;
			}
			footerView.Count = count.ToString ("N0"); 
			footerView.Tax = (_order.ChargeAmount + _order.TaxAmount).ToCurrency ();
			//table.TableFooterView.Bounds = new CoreGraphics.CGRect (0, -10, View.Frame.Width, table.TableFooterView.Bounds.Height);
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}
	}
}


