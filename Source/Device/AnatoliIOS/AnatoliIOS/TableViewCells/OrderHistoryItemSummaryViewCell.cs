using System;

using Foundation;
using UIKit;
using Anatoli.App.Model.Store;
using Anatoli.Framework.AnatoliBase;
using Anatoli.App.Manager;
using SDWebImage;

namespace AnatoliIOS.TableViewCells
{
	public partial class OrderHistoryItemSummaryViewCell : BaseTableViewCell
	{
		public static readonly NSString Key = new NSString ("OrderHistoryItemSummaryViewCell");
		public static readonly UINib Nib;

		static OrderHistoryItemSummaryViewCell ()
		{
			Nib = UINib.FromName ("OrderHistoryItemSummaryViewCell", NSBundle.MainBundle);
		}

		public OrderHistoryItemSummaryViewCell (IntPtr handle) : base (handle)
		{
		}

		public void UpdateCell(OrderItemModel item){
			productNameLabel.Text = item.product_name;
			itemPriceLabel.Text = item.item_count + " عدد (" + item.item_price.ToCurrency () + " تومان )";
			var imgUri = ProductManager.GetImageAddress(item.product_id, item.image);
			if (imgUri != null)
			{
				try
				{
					using (var url = new NSUrl(imgUri))
					{
						productImageView.SetImage(url: url);
					}
				}
				catch (Exception)
				{

				}
			}

		}
		public void BindCell (OrderItemModel item){
			addButton.TouchUpInside += async(object sender, EventArgs e) => {
				var result = await ShoppingCardManager.AddProductAsync(item.product_id,item.item_count);
				if (result) {
					var alert = UIAlertController.Create("","کالای مورد نظر به سبد خرید اضافه شد",UIAlertControllerStyle.Alert);
					alert.AddAction(UIAlertAction.Create("باشه",UIAlertActionStyle.Default,null));
					AnatoliApp.GetInstance().PresentViewController(alert);
				}
			};
		}
	}
}
