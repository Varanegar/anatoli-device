using System;

using UIKit;
using System.Drawing;
using System.Collections.Generic;
using XpandItComponents;
using CoreGraphics;
using Foundation;
using Anatoli.App.Model.Product;
using Anatoli.App.Manager;

namespace AnatoliIOS.ViewControllers
{
	public partial class FirstPageViewController : ParallaxViewController 
    {
        public FirstPageViewController()
			: base("anatoli.vndev@gmail.com","huh@vsp5BuklBGnthps5jvt")
        {
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			StartAutomaticScroll ();
			NavigationItem.RightBarButtonItems = new UIBarButtonItem[2] {AnatoliApp.GetInstance ().CreateMenuButton (), AnatoliApp.GetInstance().CreateBasketButton()};
            EdgesForExtendedLayout = UIRectEdge.None;

        }
        public async override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Perform any additional setup after loading the view, typically from a nib.
			Title = "صفحه خانگی";

			SetImageHeight(UIScreen.MainScreen.Bounds.Size.Height * 0.4f);

			// Creting a list UIImages to present in the ParallaxViewController
			var images = new List<UIImage>();
			images.Add(UIImage.FromBundle("splash"));
			images.Add(UIImage.FromBundle("splash"));
			images.Add(UIImage.FromBundle("splash"));

			//View will be the ContentView of ParallaxViewController
			var view = new UIView(new CGRect(0, 0, UIScreen.MainScreen.Bounds.Size.Width, UIScreen.MainScreen.Bounds.Size.Height));

			view.BackgroundColor = UIColor.White;
		
			//You can check if the image is tapped by set the ImageTapped property
			ImageTaped = (i) =>
			{
				UIAlertView alertView = new UIAlertView("Image tapped", "Image at index " + i, null, "Ok", null);
				alertView.Show();
			};

			SetupFor(view);
			SetImages(images);

			//groupsCollectionViewHeight.Constant = UIScreen.MainScreen.Bounds.Height * 0.5f;
			var groups = await CategoryManager.GetFirstLevelAsync ();
			var layout = new UICollectionViewFlowLayout ();
			layout.ItemSize = new CGSize (120f, 120f);
			var groupsCollectionView = new UICollectionView(new CGRect(0, 0, UIScreen.MainScreen.Bounds.Size.Width, UIScreen.MainScreen.Bounds.Size.Height * 0.6f),layout);
			groupsCollectionView.BackgroundColor = UIColor.White;
			groupsCollectionView.CollectionViewLayout = layout;
			layout.SectionInset = new UIEdgeInsets (30,30,30,30);
			groupsCollectionView.RegisterNibForCell(UINib.FromName(ProductGroupCollectionViewCell.Key, null), ProductGroupCollectionViewCell.Key);
			groupsCollectionView.Source = new ProductGroupsCollectionViewSource (groups);
			groupsCollectionView.ReloadData ();
			view.AddSubview (groupsCollectionView);
        }
    }

	class ProductGroupsCollectionViewSource : UICollectionViewSource{
		List<CategoryInfoModel> _items;
		public ProductGroupsCollectionViewSource(List<CategoryInfoModel> items){
			_items = items;
		}
		public override nint GetItemsCount (UICollectionView collectionView, nint section)
		{
			return _items.Count;
		}
		public override UICollectionViewCell GetCell (UICollectionView collectionView, NSIndexPath indexPath)
		{
			var cell = collectionView.DequeueReusableCell (ProductGroupCollectionViewCell.Key, indexPath) as ProductGroupCollectionViewCell;
			cell.UpdateCell (_items [indexPath.Row]);
			return cell;
		}
		public override void ItemSelected (UICollectionView collectionView, NSIndexPath indexPath)
		{
			var productsViewController = new ProductsViewController ();
			productsViewController.GroupId = _items [indexPath.Row].cat_id;
			AnatoliApp.GetInstance ().PushViewController (productsViewController);
		}
		public override nint NumberOfSections (UICollectionView collectionView)
		{
			return 1;
		}
	}


}