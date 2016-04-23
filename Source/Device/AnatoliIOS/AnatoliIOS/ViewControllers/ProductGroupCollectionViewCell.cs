using System;

using Foundation;
using UIKit;
using Anatoli.App.Model.Product;
using SDWebImage;
using Anatoli.App.Manager;

namespace AnatoliIOS
{
	public partial class ProductGroupCollectionViewCell : UICollectionViewCell
	{
		public static readonly NSString Key = new NSString ("ProductGroupCollectionViewCell");
		public static readonly UINib Nib;

		static ProductGroupCollectionViewCell ()
		{
			Nib = UINib.FromName ("ProductGroupCollectionViewCell", NSBundle.MainBundle);
		}

		public ProductGroupCollectionViewCell (IntPtr handle) : base (handle)
		{
		}

		public void UpdateCell(CategoryInfoModel item){
			groupImageView.Image = UIImage.FromBundle("igicon");
			groupNameLable.Text = item.cat_name;
			var imgUri = CategoryManager.GetImageAddress(item.cat_id,item.cat_image);
			if (imgUri != null) {
				try {
					using (var url = new NSUrl (imgUri)) {
						groupImageView.SetImage (url : url, placeholder: UIImage.FromBundle ("igicon"));
					}


				} catch (Exception) {

				}
			}

		}

	}
}
