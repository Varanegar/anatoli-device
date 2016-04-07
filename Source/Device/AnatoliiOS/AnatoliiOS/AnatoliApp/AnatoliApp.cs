﻿using System;
using System.Collections.Generic;
using System.Text;
using UIKit;
using AnatoliIOS.ViewControllers;
using Anatoli.App.Manager;
using System.Threading.Tasks;
using Anatoli.Framework.AnatoliBase;
using AnatoliIOS.Clients;
using Anatoli.App.Model.Store;
using Anatoli.App.Model.AnatoliUser;

namespace AnatoliIOS
{
	public class AnatoliApp
	{

		private LinkedList<Type> _views;
		private static AnatoliApp _instance;
		public UITableView MenuTableViewReference;
		public StoreDataModel DefaultStore;

		public Anatoli.App.Model.CustomerViewModel Customer { get; set; }

		public AnatoliUserModel User { get; set; }

		public async Task Initialize ()
		{
			try {
				Customer = await CustomerManager.ReadCustomerAsync ();
				User = await AnatoliUserManager.ReadUserInfoAsync ();
				DefaultStore = await StoreManager.GetDefaultAsync ();
			} catch (Exception) {

			}
		}

		public static AnatoliApp GetInstance ()
		{
			if (_instance == null) {
				_instance = new AnatoliApp ();
			}
			return _instance;
		}

		private AnatoliApp ()
		{
			AnatoliClient.GetInstance (new IosWebClient (), new IosSqliteClient (), new IosFileIO ());
			AnatoliClient.GetInstance ().WebClient.TokenExpire += delegate {
			};
			_views = new LinkedList<Type> ();
		}

		public async Task SyncDataBase ()
		{
			var updateTime = await SyncManager.GetLogAsync (SyncManager.UpdateCompleted);
			if (updateTime < (DateTime.Now - TimeSpan.FromDays (1000))) {
				try {
					SyncManager.ProgressChanged += (status, step) => {
						Console.WriteLine (status);
					};
					await SyncManager.SyncDatabase ();

				} catch (Exception ex) {
					Console.WriteLine (ex.Message);
				}
			}
		}

		public void ReplaceViewController (UIViewController viewController)
		{
			var allviewControllers = (UIApplication.SharedApplication.Delegate as AppDelegate).RootViewController.NavController.ViewControllers;
			UIViewController[] newViewControllerStack = new UIViewController[allviewControllers.Length - 1];
			for (int i = 0; i < allviewControllers.Length - 1; i++) {
				newViewControllerStack [i] = allviewControllers [i];
			}
			(UIApplication.SharedApplication.Delegate as AppDelegate).RootViewController.NavController.ViewControllers = newViewControllerStack;
			PushViewController (viewController);
		}

		public void PresentViewController (UIViewController view, bool animated = true, Action completedAction = null)
		{
			(UIApplication.SharedApplication.Delegate as AppDelegate).RootViewController.NavController.PresentViewController (view, animated, completedAction);
		}

		public void PushViewController (UIViewController viewController, bool force = false)
		{
			if (viewController == null) {
				throw new ArgumentNullException ();
			} else if ((_views.Count > 0 && _views.Last.Value != viewController.GetType ()) || force) {
				(UIApplication.SharedApplication.Delegate as AppDelegate).RootViewController.NavController.PushViewController (viewController, true);
				_views.AddLast (viewController.GetType ());
			} else if (_views.Count == 0) {
				(UIApplication.SharedApplication.Delegate as AppDelegate).RootViewController.NavController.PushViewController (viewController, true);
				_views.AddLast (viewController.GetType ());
			}
		}

		public bool PopViewController ()
		{
			if (_views.Count > 0) {
				_views.RemoveLast ();
				return true;
			} else
				return false;
		}
		public async Task RefreshMenu(string catId){
			var source = new MenuTableViewSource ();
			source.Items = new System.Collections.Generic.List<MenuItem> ();
			source.Items.Add (new MenuItem () {
				Title = " منوی اصلی",
				Type = MenuItem.MenuType.MainMenu
			});
			if (catId == "0") {
				var cats = await CategoryManager.GetFirstLevelAsync ();
				foreach (var item in cats) {
					source.Items.Add (new MenuItem () {
						Title = item.cat_name,
						Type = MenuItem.MenuType.CatId,
						Id = item.cat_id
					});
				}
			} else {
				var cats = await CategoryManager.GetCategoriesAsync (catId);
				var parent = await CategoryManager.GetParentCategoryAsync (catId);
				var current = await CategoryManager.GetCategoryInfoAsync (catId);
				source.Items.Add (new MenuItem () {
					Title = "همه محصولات",
					Type = MenuItem.MenuType.CatId,
					Id = "0"
				});
				if (parent != null) {
					source.Items.Add (new MenuItem () {
						Title = parent.cat_name,
						Type = MenuItem.MenuType.CatId,
						Id = parent.cat_id
					});
				}
				source.Items.Add (new MenuItem () {
					Title = current.cat_name,
					Type = MenuItem.MenuType.CatId,
					Id = catId
				});
				foreach (var item in cats) {
					source.Items.Add (new MenuItem () {
						Title = item.cat_name,
						Type = MenuItem.MenuType.CatId,
						Id = item.cat_id
					});
				}
			}
			MenuTableViewReference.Source = source;
			MenuTableViewReference.ReloadData ();
		}

		public void RefreshMenu ()
		{
			
			var source = new MenuTableViewSource ();
			source.Items = new System.Collections.Generic.List<MenuItem> ();

			if (Customer == null) {
				source.Items.Add (new MenuItem () {
					Title = "ورود ",
					Icon = UIImage.FromBundle ("ic_log_in_green_24dp.png"),
					Type = MenuItem.MenuType.Login
				});
			} else {
				if (Customer != null && Customer.FirstName != null && Customer.LastName != null) {
					source.Items.Add (new MenuItem () {
						Title = Customer.FirstName + " " + Customer.LastName,
						Icon = UIImage.FromBundle ("ic_person_gray_24dp"),
						Type = MenuItem.MenuType.Profile
					});
				} else {
					source.Items.Add (new MenuItem () {
						Title = "پروفایل ",
						Icon = UIImage.FromBundle ("ic_person_gray_24dp"),
						Type = MenuItem.MenuType.Profile
					});
				}
			}
			source.Items.Add (new MenuItem () {
				Title = "صفحه اول",
				Type = MenuItem.MenuType.FirstPage
			});
			source.Items.Add (new MenuItem () {
				Title = "دسته بندی کالا ",
				Icon = UIImage.FromBundle ("ic_list_orange_24dp"),
				Type = MenuItem.MenuType.Products
			});
					
			if(Customer != null)
				source.Items.Add (new MenuItem () {
					Title = " لیست من",
					Type = MenuItem.MenuType.Favorits
				});
			
			if (DefaultStore == null)
				source.Items.Add (new MenuItem () {
					Title = " فروشگاه من",
					Type = MenuItem.MenuType.Stores
				});
			else
				source.Items.Add (new MenuItem () {
					Title = " فروشگاه من" + "(" + DefaultStore.store_name + ")",
					Type = MenuItem.MenuType.Stores
				});
			MenuTableViewReference.Source = source;
			MenuTableViewReference.ReloadData ();
		}

		public void CloseMenu(){
			(UIApplication.SharedApplication.Delegate as AppDelegate).RootViewController.SidebarController.CloseMenu();
		}
		public void OpenMenu(){
			(UIApplication.SharedApplication.Delegate as AppDelegate).RootViewController.SidebarController.OpenMenu();
		}
		public async Task SelectMenuItemAsync (int index)
		{
			var items = (MenuTableViewReference.Source as MenuTableViewSource).Items;
			switch (items [index].Type) {
			case MenuItem.MenuType.FirstPage:
				PushViewController (new FirstPageViewController ());
				CloseMenu ();
				break;
			case MenuItem.MenuType.Favorits:
				PushViewController (new FavoritsViewController ());
				CloseMenu ();
				break;
			case MenuItem.MenuType.Login:
				PushViewController (new LoginViewController ());
				CloseMenu ();
				break;
			case MenuItem.MenuType.Profile:
				PushViewController (new ProfileViewController ());
				CloseMenu ();
				break;
			case MenuItem.MenuType.Products:
				if (DefaultStore != null) {
					await RefreshMenu ("0");
					PushViewController (new ProductsViewController (),true);
				} else {
					PushViewController (new StoresViewController ());
					CloseMenu ();
				}
				break;
			case MenuItem.MenuType.Stores:
				PushViewController (new StoresViewController ());
				CloseMenu ();
				break;
			case MenuItem.MenuType.CatId:
				await RefreshMenu (items [index].Id);
				var v = new ProductsViewController ();
				v.SetGroupId (items [index].Id);
				PushViewController (v,true);
				break;
			case MenuItem.MenuType.MainMenu:
				RefreshMenu ();
				break;
			default:
				break;
			}
		}

		public async Task LogOutAsync ()
		{
			await AnatoliUserManager.LogoutAsync ();
			Customer = null;
			User = null;
			RefreshMenu ();
			ReplaceViewController (new FirstPageViewController ());
		}


		public UIBarButtonItem CreateMenuButton ()
		{
			return new UIBarButtonItem (UIImage.FromBundle ("ic_reorder_white_24dp").Scale (new CoreGraphics.CGSize (26, 26))
				, UIBarButtonItemStyle.Plain
				, (sender, args) => {
				(UIApplication.SharedApplication.Delegate as AppDelegate).RootViewController.SidebarController.ToggleMenu ();
			});
		}

		public UIBarButtonItem CreateBasketButton ()
		{
			return new UIBarButtonItem (UIImage.FromBundle ("ic_shoppingcard_on_white_24dp").Scale (new CoreGraphics.CGSize (26, 26))
				, UIBarButtonItemStyle.Plain
				, (sender, args) => {
				if (Customer == null) {
					var loginAlert = UIAlertController.Create ("خطا", "لطفا ابتدا وارد حساب کاربری خود شوید", UIAlertControllerStyle.Alert);
					loginAlert.AddAction (UIAlertAction.Create ("باشه", UIAlertActionStyle.Default,
						delegate {
							PushViewController (new LoginViewController ());
						}
					));
					loginAlert.AddAction (UIAlertAction.Create ("بی خیال", UIAlertActionStyle.Cancel, null));
					PresentViewController (loginAlert);
				} else if (DefaultStore == null) {
					var storeAlert = UIAlertController.Create ("خطا", "لطفا ابتدا فروشگاه پیش فرض را انتخاب نمایید", UIAlertControllerStyle.Alert);
					storeAlert.AddAction (UIAlertAction.Create ("باشه", UIAlertActionStyle.Default,
						delegate {
							PushViewController (new StoresViewController ());
						}
					));
					storeAlert.AddAction (UIAlertAction.Create ("بی خیال", UIAlertActionStyle.Cancel, null));
					PresentViewController (storeAlert);
				} else
					AnatoliApp.GetInstance ().PushViewController (new ShoppingCardViewController ());
			});
		}

	}

	public static class Extensions
	{
		public static UIColor FromHex (this UIColor color, int hexValue)
		{
			return UIColor.FromRGB (
				(((float)((hexValue & 0xFF0000) >> 16)) / 255.0f),
				(((float)((hexValue & 0xFF00) >> 8)) / 255.0f),
				(((float)(hexValue & 0xFF)) / 255.0f)
			);
		}
	}


}
