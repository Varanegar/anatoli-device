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

namespace AnatoliIOS.TableViewCells
{
	[Register ("OrderHistoryItemSummaryViewCell")]
	partial class OrderHistoryItemSummaryViewCell
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton addButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel itemPriceLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIImageView productImageView { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel productNameLabel { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (addButton != null) {
				addButton.Dispose ();
				addButton = null;
			}
			if (itemPriceLabel != null) {
				itemPriceLabel.Dispose ();
				itemPriceLabel = null;
			}
			if (productImageView != null) {
				productImageView.Dispose ();
				productImageView = null;
			}
			if (productNameLabel != null) {
				productNameLabel.Dispose ();
				productNameLabel = null;
			}
		}
	}
}
