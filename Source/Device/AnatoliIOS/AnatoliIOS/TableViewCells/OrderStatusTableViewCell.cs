using System;

using Foundation;
using UIKit;
using Anatoli.App.Model.Store;
using Anatoli.Framework.AnatoliBase;

namespace AnatoliIOS.TableViewCells
{
	public partial class OrderStatusTableViewCell : UITableViewCell
	{
		public static readonly NSString Key = new NSString ("OrderStatusTableViewCell");
		public static readonly UINib Nib;

		static OrderStatusTableViewCell ()
		{
			Nib = UINib.FromName ("OrderStatusTableViewCell", NSBundle.MainBundle);
		}

		public OrderStatusTableViewCell (IntPtr handle) : base (handle)
		{
		}
		public void Update(OrderModel item){
			orderNoLabel.Text = "شماره سفارش : " + item.order_id;
			orderDateLabel.Text = "تاریخ : " + item.order_date;
			orderStatusLabel.Text = "وضعیت سفارش : " + PurchaseOrderStatusHistoryViewModel.GetStatusName (item.order_status);
			storeNameLabel.Text = "فروشگاه : " + item.store_name;
			orderPriceLabel.Text = "مبلغ : " + item.order_price.ToCurrency () + " تومان";
		}
	}
}
