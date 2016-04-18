// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace AnatoliIOS.ViewControllers
{
	[Register ("FooterView")]
	partial class FooterView
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel totalCountLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel totalDiscountLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel totalPriceLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel totalTaxLabel { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (totalCountLabel != null) {
				totalCountLabel.Dispose ();
				totalCountLabel = null;
			}
			if (totalDiscountLabel != null) {
				totalDiscountLabel.Dispose ();
				totalDiscountLabel = null;
			}
			if (totalPriceLabel != null) {
				totalPriceLabel.Dispose ();
				totalPriceLabel = null;
			}
			if (totalTaxLabel != null) {
				totalTaxLabel.Dispose ();
				totalTaxLabel = null;
			}
		}
	}
}
