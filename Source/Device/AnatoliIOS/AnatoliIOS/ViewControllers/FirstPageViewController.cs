using System;

using UIKit;
using System.Drawing;
using System.Collections.Generic;
using XpandItComponents;
using CoreGraphics;
using Foundation;
using Anatoli.App.Model.Product;
using Anatoli.App.Manager;
using SDWebImage;

namespace AnatoliIOS.ViewControllers
{
    public partial class FirstPageViewController : ParallaxViewController
    {
        public FirstPageViewController()
            : base("anatoli.vndev@gmail.com", "huh@vsp5BuklBGnthps5jvt")
        {
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            NavigationItem.BackBarButtonItem = new UIBarButtonItem("", UIBarButtonItemStyle.Plain, null);
            NavigationItem.HidesBackButton = true;
            StartAutomaticScroll();
            EdgesForExtendedLayout = UIRectEdge.None;
            this.SetToolbarItems(AnatoliApp.GetInstance().CreateToolbarItems(), true);
            this.NavigationController.ToolbarHidden = false;
            AnatoliApp.GetInstance().SyncDataBase();
        }
        public async override void ViewDidLoad()
        {
            base.ViewDidLoad();
            EdgesForExtendedLayout = UIRectEdge.None;
            // Perform any additional setup after loading the view, typically from a nib.
            Title = "صفحه خانگی";

			var searchButton = new UIBarButtonItem(UIImage.FromBundle("ic_search_white_24dp").Scale(new CGSize(26, 26)),
				UIBarButtonItemStyle.Plain,
				(sender, args) =>
				{
					var p = new ProductsViewController();
					p.SearchBar = true; 
					AnatoliApp.GetInstance().PushViewController(p);
				});
			NavigationItem.SetRightBarButtonItem(searchButton, true);

            var bannerHeight = UIScreen.MainScreen.Bounds.Size.Height * 0.45f;
            SetImageHeight(bannerHeight);
            // Creting a list UIImages to present in the ParallaxViewController
            var images = new List<UIImage>();
            View.BackgroundColor = UIColor.White;

            SDWebImageDownloader.SharedDownloader.DownloadImage(
                new NSUrl("http://parastoo.varanegar.com:7000/Content/Images/F739D5DD-8B18-42DD-9E16-910EF346B365/original/slide-1.jpg"),
                SDWebImageDownloaderOptions.LowPriority,
                (receivedSize, expectedSize) =>
                {
                    // Track progress...
                },
                (image, data, error, finished) =>
                {
                    if (image != null && finished)
                    {
                        InvokeOnMainThread(() => { images.Add(image); SetImages(images); });
                    }
                }
            );
            SDWebImageDownloader.SharedDownloader.DownloadImage(
                new NSUrl("http://parastoo.varanegar.com:7000/Content/Images/F739D5DD-8B18-42DD-9E16-910EF346B365/original/slide-2.jpg"),
                SDWebImageDownloaderOptions.LowPriority,
                (receivedSize, expectedSize) =>
                {
                    // Track progress...
                },
                (image, data, error, finished) =>
                {
                    if (image != null && finished)
                    {
                        InvokeOnMainThread(() => { images.Add(image); SetImages(images); });
                    }
                }
            );
            SDWebImageDownloader.SharedDownloader.DownloadImage(
                new NSUrl("http://parastoo.varanegar.com:7000/Content/Images/F739D5DD-8B18-42DD-9E16-910EF346B365/original/slide-3.jpg"),
                SDWebImageDownloaderOptions.LowPriority,
                (receivedSize, expectedSize) =>
                {
                    // Track progress...
                },
                (image, data, error, finished) =>
                {
                    if (image != null && finished)
                    {
                        InvokeOnMainThread(() => { images.Add(image); SetImages(images); });
                    }
                }
            );


            //View will be the ContentView of ParallaxViewController
            var view = new UIView(new CGRect(0, 0, UIScreen.MainScreen.Bounds.Size.Width, 1300));
            view.BackgroundColor = UIColor.White;
            //You can check if the image is tapped by set the ImageTapped property
            ImageTaped = (i) =>
            {
                UIAlertView alertView = new UIAlertView("Image tapped", "Image at index " + i, null, "Ok", null);
                alertView.Show();
            };
            SetupFor(view);
            var groups = ProductGroupManager.GetFirstLevel();
            var layout = new UICollectionViewFlowLayout();
            groups = ProductGroupManager.GetFirstLevel();
            layout.ItemSize = new CGSize(120f, 120f);
            var groupsCollectionView = new UICollectionView(new CGRect(0, 0, UIScreen.MainScreen.Bounds.Size.Width, (groups.Count + 1) / 2 * 150f), layout);
            groupsCollectionView.BackgroundColor = UIColor.White;
            groupsCollectionView.CollectionViewLayout = layout;
            layout.SectionInset = new UIEdgeInsets(30, 30, 30, 30);
            groupsCollectionView.RegisterNibForCell(UINib.FromName(ProductGroupCollectionViewCell.Key, null), ProductGroupCollectionViewCell.Key);
            groupsCollectionView.Source = new ProductGroupsCollectionViewSource(groups);
            groupsCollectionView.ReloadData();
            view.AddSubview(groupsCollectionView);
        }
    }

    class ProductGroupsCollectionViewSource : UICollectionViewSource
    {
        List<ProductGroupModel> _items;
        public ProductGroupsCollectionViewSource(List<ProductGroupModel> items)
        {
            _items = items;
        }
        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return _items.Count;
        }
        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var cell = collectionView.DequeueReusableCell(ProductGroupCollectionViewCell.Key, indexPath) as ProductGroupCollectionViewCell;
            cell.UpdateCell(_items[indexPath.Row]);
            return cell;
        }
        public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var productsViewController = new ProductsViewController();
            productsViewController.GroupId = _items[indexPath.Row].UniqueId;
            AnatoliApp.GetInstance().PushViewController(productsViewController);
        }
        public override nint NumberOfSections(UICollectionView collectionView)
        {
            return 1;
        }
    }


}