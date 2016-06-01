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
	[Register ("StoreSummaryTableViewCell")]
	partial class StoreSummaryTableViewCell
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton locationButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel storeAddressLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIImageView storeImageView { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel storeNameLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel storeStatusLabel { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (locationButton != null) {
				locationButton.Dispose ();
				locationButton = null;
			}
			if (storeAddressLabel != null) {
				storeAddressLabel.Dispose ();
				storeAddressLabel = null;
			}
			if (storeImageView != null) {
				storeImageView.Dispose ();
				storeImageView = null;
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
