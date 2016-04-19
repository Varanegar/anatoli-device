using System;

using UIKit;

namespace AnatoliIOS.ViewControllers
{
    public partial class ConfirmRegisterationViewController : UIViewController
    {
        private string _username;
        private string _password;

        public ConfirmRegisterationViewController()
            : base("ConfirmRegisterationViewController", null)
        {
        }

        public ConfirmRegisterationViewController(string username, string password)
        {
            // TODO: Complete member initialization
            _username = username;
            _password = password;
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            
        }
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Perform any additional setup after loading the view, typically from a nib.
        }
    }
}