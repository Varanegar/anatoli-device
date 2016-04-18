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
	[Register ("OrderItemTableViewCell")]
	partial class OrderItemTableViewCell
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel itemCountLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel itemNameLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel itemPriceLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel itemRowLabel { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (itemCountLabel != null) {
				itemCountLabel.Dispose ();
				itemCountLabel = null;
			}
			if (itemNameLabel != null) {
				itemNameLabel.Dispose ();
				itemNameLabel = null;
			}
			if (itemPriceLabel != null) {
				itemPriceLabel.Dispose ();
				itemPriceLabel = null;
			}
			if (itemRowLabel != null) {
				itemRowLabel.Dispose ();
				itemRowLabel = null;
			}
		}
	}
}
