using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using ObjCRuntime;

namespace AnatoliIOS.ViewControllers
{
	partial class FooterView : UIView
	{
		public string Count{
			get{ return  totalCountLabel.Text;}
			set{ totalCountLabel.Text = value;}
		}
		public string Discount{
			get {return totalDiscountLabel.Text;}
			set{ totalDiscountLabel.Text = value;}
		}
		public string Price{
			get{ return totalPriceLabel.Text;}
			set{ totalPriceLabel.Text = value;}
		}
		public string Tax{
			get{ return totalTaxLabel.Text;}
			set{ totalTaxLabel.Text = value;}
		}
		public FooterView (IntPtr handle) : base (handle)
		{
		}
		public static FooterView Create(){
			var arr = NSBundle.MainBundle.LoadNib ("ProformaFooter", null, null);
			var v = Runtime.GetNSObject<FooterView> (arr.ValueAt(0));
			return v;
		}
	}
}
