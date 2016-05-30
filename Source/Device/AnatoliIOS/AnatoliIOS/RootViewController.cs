using AnatoliIOS.ViewControllers;
using Foundation;
using SidebarNavigation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using Anatoli.App.Manager;
using Anatoli.Framework.AnatoliBase;

namespace AnatoliIOS
{
    public partial class RootViewController : UIViewController
    {
        // the sidebar controller for the app
        public SidebarController SidebarController { get; private set; }
        // the navigation controller
        public NavController NavController { get; private set; }
        public RootViewController(IntPtr handle)
            : base(handle)
        {
        }
        public async override void ViewDidLoad()
        {
            base.ViewDidLoad();
            NavigationItem.BackBarButtonItem = new UIBarButtonItem ("", UIBarButtonItemStyle.Plain, null);
            await AnatoliApp.GetInstance().Initialize();
            NavController = new NavController();
            SidebarController = new SidebarNavigation.SidebarController(this, NavController, new SideMenuController());
            SidebarController.MenuWidth = 180;
            SidebarController.ReopenOnRotate = false;
            if (AnatoliApp.GetInstance().DefaultStore == null)
            {
                AnatoliApp.GetInstance().PushViewController(new StoresViewController());
            }
            else
                AnatoliApp.GetInstance().PushViewController(new FirstPageViewController());
        }
        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}
