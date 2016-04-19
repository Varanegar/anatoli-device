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
	[Register ("LoginViewController")]
	partial class LoginViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton forgotPasswordButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton loginButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField passwordTextField { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton registerButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField userNameTextField { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (forgotPasswordButton != null) {
				forgotPasswordButton.Dispose ();
				forgotPasswordButton = null;
			}
			if (loginButton != null) {
				loginButton.Dispose ();
				loginButton = null;
			}
			if (passwordTextField != null) {
				passwordTextField.Dispose ();
				passwordTextField = null;
			}
			if (registerButton != null) {
				registerButton.Dispose ();
				registerButton = null;
			}
			if (userNameTextField != null) {
				userNameTextField.Dispose ();
				userNameTextField = null;
			}
		}
	}
}
