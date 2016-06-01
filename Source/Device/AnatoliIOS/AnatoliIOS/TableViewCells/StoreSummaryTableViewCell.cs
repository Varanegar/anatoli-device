using System;

using Foundation;
using UIKit;
using Anatoli.App.Model.Store;

namespace AnatoliIOS.TableViewCells
{
	public partial class StoreSummaryTableViewCell : UITableViewCell
	{
		public static readonly NSString Key = new NSString ("StoreSummaryTableViewCell");
		public static readonly UINib Nib;

		static StoreSummaryTableViewCell ()
		{
			Nib = UINib.FromName ("StoreSummaryTableViewCell", NSBundle.MainBundle);
		}

		public StoreSummaryTableViewCell (IntPtr handle) : base (handle)
		{
		}

		public void UpdateCell(StoreDataModel item){
			if (item != null) {
				storeNameLabel.Text = item.store_name;
				storeAddressLabel.Text = item.store_address;
                storeStatusLabel.Text = "باز است";
				storeStatusLabel.Layer.BorderColor = UIColor.Black.CGColor;
				storeStatusLabel.Layer.BorderWidth = 2.0f;
				locationButton.TouchUpInside += (object sender, EventArgs e) => {
					NSUrl url = new NSUrl("http://maps.apple.com/?ll=" + item.location);
					if (UIApplication.SharedApplication.CanOpenUrl(url) && !string.IsNullOrEmpty(item.location)) {
						UIApplication.SharedApplication.OpenUrl(url);
					}else{
						new UIAlertView("خطا","آدرس دقیق وجود ندارد",null,"باشه").Show();
					}
				};
			}
		}

	}
}
