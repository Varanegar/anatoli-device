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
	[Register ("OrderStatusTableViewCell")]
	partial class OrderStatusTableViewCell
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel orderDateLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel orderNoLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel orderPriceLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel orderStatusLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel storeNameLabel { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (orderDateLabel != null) {
				orderDateLabel.Dispose ();
				orderDateLabel = null;
			}
			if (orderNoLabel != null) {
				orderNoLabel.Dispose ();
				orderNoLabel = null;
			}
			if (orderPriceLabel != null) {
				orderPriceLabel.Dispose ();
				orderPriceLabel = null;
			}
			if (orderStatusLabel != null) {
				orderStatusLabel.Dispose ();
				orderStatusLabel = null;
			}
			if (storeNameLabel != null) {
				storeNameLabel.Dispose ();
				storeNameLabel = null;
			}
		}
	}
}
