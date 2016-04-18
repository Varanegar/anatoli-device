using System;

using Foundation;
using UIKit;
using Anatoli.App.Model.Store;

namespace AnatoliIOS.TableViewCells
{
	public partial class RegionChooserTableViewCell : UITableViewCell
	{
		public static readonly NSString Key = new NSString ("RegionChooserTableViewCell");
		public static readonly UINib Nib;

		static RegionChooserTableViewCell ()
		{
			Nib = UINib.FromName ("RegionChooserTableViewCell", NSBundle.MainBundle);
		}

		public RegionChooserTableViewCell (IntPtr handle) : base (handle)
		{
		}
		public void Update(CityRegionModel item){
			titleLabel.Text = item.group_name;
		}
	}
}
