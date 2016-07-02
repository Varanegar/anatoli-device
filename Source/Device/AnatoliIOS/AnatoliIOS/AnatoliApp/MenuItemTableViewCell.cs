using System;

using Foundation;
using UIKit;
using CoreGraphics;
using System.Drawing;
using Anatoli.App.Manager;
using System.Threading.Tasks;

namespace AnatoliIOS
{
	public partial class MenuItemTableViewCell : UITableViewCell
	{
		public static readonly NSString Key = new NSString ("MenuItemTableViewCell");
		public static readonly UINib Nib;

		static MenuItemTableViewCell ()
		{
			Nib = UINib.FromName ("MenuItemTableViewCell", NSBundle.MainBundle);
		}

		public MenuItemTableViewCell (IntPtr handle) : base (handle)
		{
			
		}

		public async Task UpdateCellAsync(MenuItem item){
			titleLabel.Text = item.Title;
			iconImageView.Image = item.Icon;
			if (item.Type == MenuItem.MenuType.CatId) {
				var info = await ProductGroupManager.GetGroupInfo(item.Id);
				if (info != null) {
					var x = titleLabel.Center.X - (10f * info.cat_depth);
					titleLabel.Center = new CGPoint (x, titleLabel.Center.Y);
					titleLabel.Font = UIFont.FromName ("IRAN", 13);
				}
				BackgroundColor = item.Color;
			}
		}
	}
}
