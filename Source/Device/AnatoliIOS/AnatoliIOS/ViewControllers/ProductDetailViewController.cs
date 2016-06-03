using System;

using UIKit;
using Anatoli.App.Manager;
using Anatoli.App.Model.Product;
using Foundation;
using Anatoli.Framework.AnatoliBase;
using SDWebImage;
using System.Collections.Generic;
using Anatoli.App.Model.Store;

namespace AnatoliIOS
{
    public partial class ProductDetailViewController : UIViewController
    {
        ProductModel _product;
        public ProductDetailViewController()
            : base("ProductDetailViewController", null)
        {
        }
        public ProductDetailViewController(ProductModel product)
            : this()
        {
            _product = product;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
        }
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
			EdgesForExtendedLayout = UIRectEdge.None;
			counterView.SetBorders ();
            var imgUri = ProductManager.GetImageAddress(_product.product_id, _product.image);
            if (imgUri != null)
            {
				Console.WriteLine (imgUri);
                try
                {
                    using (var url = new NSUrl(imgUri))
                    {
                        productImageView.SetImage(url: url, placeholder: UIImage.FromBundle("igicon"));
                    }
                }
                catch (Exception)
                {
                }
            }
            productTitleLabel.Text = _product.product_name;
            if (!_product.IsAvailable)
            {
                addButton.Enabled = false;
                productTitleLabel.TextColor = UIColor.Gray;
                productPriceLabel.TextColor = UIColor.Gray;
                productPriceLabel.Text = "موجود نیست";
            }
            else
            {
                addButton.Enabled = true;
                productTitleLabel.TextColor = UIColor.Black;
                productPriceLabel.TextColor = UIColor.Black;
                productPriceLabel.Text = _product.price.ToCurrency() + " تومان";
            }
            if (_product.count > 0)
            {
                counterView.Hidden = false;
            }
            else
                counterView.Hidden = true;
            productGroupLabel.Text = _product.cat_name;
            orderCountLabel.Text = _product.count + " عدد";
            addButton.TouchUpInside += async (object sender, EventArgs e) =>
            {
                addButton.Enabled = false;
                if (_product.count + 1 > _product.qty)
                {
                    var alert = UIAlertController.Create("خطا", "موجودی کافی نیست", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default, null));
                    AnatoliApp.GetInstance().PresentViewController(alert);
                    addButton.Enabled = true;
                    return;
                }
                var result = await ShoppingCardManager.AddProductAsync(_product);
                addButton.Enabled = true;
                if (result)
                {
                    counterView.Hidden = false;
                    orderCountLabel.Text = _product.count.ToString() + " عدد";
                }
            };
            removeButton.TouchUpInside += async (object sender, EventArgs e) =>
            {
                if (_product.count > 0)
                {
                    var result = await ShoppingCardManager.RemoveProductAsync(_product);
                    if (result)
                    {
                        orderCountLabel.Text = _product.count.ToString() + " عدد";
                        if (_product.count == 0)
                        {
                            counterView.Hidden = true;
                        }
                    }
                }
            };
            backButton.TouchUpInside += delegate
            {
                DismissViewController(true, null);
            };
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }

}


