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
	[Register ("OrderDetailViewController")]
	partial class OrderDetailViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITableView itemsTableView { get; set; }

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
		UILabel storeNameLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel storeStatusLabel { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (itemsTableView != null) {
				itemsTableView.Dispose ();
				itemsTableView = null;
			}
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
			if (storeNameLabel != null) {
				storeNameLabel.Dispose ();
				storeNameLabel = null;
			}
			if (storeStatusLabel != null) {
				storeStatusLabel.Dispose ();
				storeStatusLabel = null;
			}
		}
	}
}
