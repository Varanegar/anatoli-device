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
	[Register ("SendConfirmCodeViewController")]
	partial class SendConfirmCodeViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField codeTextField { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField password2EditText { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField passwordEditText { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton sendButton { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (codeTextField != null) {
				codeTextField.Dispose ();
				codeTextField = null;
			}
			if (password2EditText != null) {
				password2EditText.Dispose ();
				password2EditText = null;
			}
			if (passwordEditText != null) {
				passwordEditText.Dispose ();
				passwordEditText = null;
			}
			if (sendButton != null) {
				sendButton.Dispose ();
				sendButton = null;
			}
		}
	}
}
