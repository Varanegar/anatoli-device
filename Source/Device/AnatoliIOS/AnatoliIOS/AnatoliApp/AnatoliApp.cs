using System;
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
using CoreGraphics;
using Anatoli.App.Model.Product;
using AnatoliIOS.Components;

namespace AnatoliIOS
{
    public class AnatoliApp
    {

        private LinkedList<Type> _views;
        private static AnatoliApp _instance;
        public UITableView MenuTableViewReference;
        public StoreDataModel DefaultStore;
        public int ShoppingCardItemsCount;
        public double ShoppingCardTotalPrice;
        UILabel _counterLabel;
        //UILabel _priceLabel;
        public Anatoli.App.Model.CustomerViewModel Customer { get; set; }

        public AnatoliUserModel User { get; set; }

        public async Task Initialize()
        {
            try
            {
                Customer = await CustomerManager.ReadCustomerAsync();
                User = await AnatoliUserManager.ReadUserInfoAsync();
                DefaultStore = await StoreManager.GetDefaultAsync();
                AnatoliClient.GetInstance().WebClient.TokenExpire += async delegate
                {
                    await AnatoliApp.GetInstance().LogOutAsync();
                    AnatoliApp.GetInstance().PushViewController(new LoginViewController());
                };
                var info = await ShoppingCardManager.GetInfoAsync();
                if (info != null)
                {
                    ShoppingCardItemsCount = info.items_count;
                    ShoppingCardTotalPrice = info.total_price;
                }
            }
            catch (Exception)
            {

            }
        }

        public static AnatoliApp GetInstance()
        {
            if (_instance == null)
            {
                _instance = new AnatoliApp();
            }
            return _instance;
        }

        private AnatoliApp()
        {
            AnatoliClient.GetInstance(new IosWebClient(), new IosSqliteClient(), new IosFileIO());
            _views = new LinkedList<Type>();
        }
        bool _updating = false;
        public async Task SyncDataBase()
        {
            if (_updating)
            {
                return;
            }
            _updating = true;
            var updateTime = await SyncManager.GetLogAsync(SyncManager.UpdateCompleted);
            if ((DateTime.Now - updateTime) > TimeSpan.FromDays(3))
            {
                if (AnatoliClient.GetInstance().WebClient.IsOnline())
                {
                    LoadingOverlay loading = new LoadingOverlay(GetVisibleViewController().View.Bounds);
                    GetVisibleViewController().View.AddSubview(loading);
                    try
                    {
                        SyncManager.ProgressChanged += (status, step) =>
                        {
                            Console.WriteLine(status);
                            loading.Message = status;
                        };
                        await SyncManager.SyncDatabase();
                    }
                    catch (ServerUnreachableException)
                    {
                        var alert = UIAlertController.Create("خطا", "لطفا دستگاه  خود را به اینترنت متصل  کنید", UIAlertControllerStyle.Alert);
                        alert.AddAction(UIAlertAction.Create("دوباره تلاش کن", UIAlertActionStyle.Default, async delegate { await SyncDataBase(); }));
                        PresentViewController(alert);
                    }
                    catch (AnatoliWebClientException ex)
                    {
                        var alert = UIAlertController.Create("خطا", ex.MetaInfo.ModelStateString, UIAlertControllerStyle.Alert);
                        alert.AddAction(UIAlertAction.Create("دوباره تلاش کن", UIAlertActionStyle.Default, async delegate { await SyncDataBase(); }));
                        PresentViewController(alert);
                    }
                    catch (Exception)
                    {
                        var alert = UIAlertController.Create("خطا", "خطا در بروزرسانی اطلاعات", UIAlertControllerStyle.Alert);
                        alert.AddAction(UIAlertAction.Create("دوباره تلاش کن", UIAlertActionStyle.Default, async delegate { await SyncDataBase(); }));
                        PresentViewController(alert);
                    }
                    finally
                    {
                        loading.Hide();
                    }
                    _updating = false;
                    return;
                }
                else
                {
                    var alert = UIAlertController.Create("خطا", "لطفا دستگاه  خود را به اینترنت متصل  کنید", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("دوباره تلاش کن", UIAlertActionStyle.Default, async delegate { await SyncDataBase(); }));
                    PresentViewController(alert);
                }
            }
            else if (updateTime < (DateTime.Now - TimeSpan.FromHours(12)))
            {
                if (AnatoliClient.GetInstance().WebClient.IsOnline())
                {
                    try
                    {
                        SyncManager.ProgressChanged += (status, step) =>
                        {
                            Console.WriteLine(status);
                        };
                        await SyncManager.SyncDatabase();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    _updating = false;
                    return;
                }
            }
            else if (updateTime < (DateTime.Now - TimeSpan.FromMinutes(20)))
            {
                if (AnatoliClient.GetInstance().WebClient.IsOnline())
                {
                    try
                    {
                        await ProductManager.SyncOnHandAsync(null);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    _updating = false;
                    return;
                }
            }
            _updating = false;
        }

        public void ReplaceViewController(UIViewController viewController)
        {
            var allviewControllers = (UIApplication.SharedApplication.Delegate as AppDelegate).RootViewController.NavController.ViewControllers;
            UIViewController[] newViewControllerStack = new UIViewController[allviewControllers.Length - 1];
            for (int i = 0; i < allviewControllers.Length - 1; i++)
            {
                newViewControllerStack[i] = allviewControllers[i];
            }
            (UIApplication.SharedApplication.Delegate as AppDelegate).RootViewController.NavController.ViewControllers = newViewControllerStack;
            PushViewController(viewController);
        }

        public void PresentViewController(UIViewController view, bool animated = true, Action completedAction = null)
        {
            (UIApplication.SharedApplication.Delegate as AppDelegate).RootViewController.NavController.PresentViewController(view, animated, completedAction);
        }

        public void PushViewController(UIViewController viewController, bool force = false)
        {
            if (viewController == null)
            {
                throw new ArgumentNullException();
            }
            else if ((_views.Count > 0 && _views.Last.Value != viewController.GetType()) || force)
            {
                (UIApplication.SharedApplication.Delegate as AppDelegate).RootViewController.NavController.PushViewController(viewController, true);
                _views.AddLast(viewController.GetType());
            }
            else if (_views.Count == 0)
            {
                (UIApplication.SharedApplication.Delegate as AppDelegate).RootViewController.NavController.PushViewController(viewController, true);
                _views.AddLast(viewController.GetType());
            }
            if (_views.Last.Value != typeof(ProductsViewController))
            {
                RefreshMenu();
            }
        }

        public bool PopViewController()
        {
            if (_views.Count > 0)
            {
                _views.RemoveLast();
                var last = _views.Last;
                if (last.Value != typeof(ProductsViewController))
                {
                    RefreshMenu();
                }
                return true;
            }
            else
                return false;
        }
        public UIViewController GetVisibleViewController()
        {
            return (UIApplication.SharedApplication.Delegate as AppDelegate).RootViewController.NavController.TopViewController;
        }
        public async Task RefreshMenu(string catId)
        {
            var source = new MenuTableViewSource();
            source.Items = new System.Collections.Generic.List<MenuItem>();
            source.Items.Add(new MenuItem()
            {
                Title = " منوی اصلی",
                Type = MenuItem.MenuType.MainMenu
            });
            if (catId == "0")
            {
                var cats = await CategoryManager.GetFirstLevelAsync();
                foreach (var item in cats)
                {
                    source.Items.Add(new MenuItem()
                    {
                        Title = item.cat_name,
                        Type = MenuItem.MenuType.CatId,
                        Id = item.cat_id
                    });
                }
            }
            else
            {
                var cats = await CategoryManager.GetCategoriesAsync(catId);
                var parent = await CategoryManager.GetParentCategoryAsync(catId);
                var current = await CategoryManager.GetCategoryInfoAsync(catId);
                source.Items.Add(new MenuItem()
                {
                    Title = "همه محصولات",
                    Type = MenuItem.MenuType.CatId,
                    Id = "0"
                });
                if (parent != null)
                {
                    source.Items.Add(new MenuItem()
                    {
                        Title = parent.cat_name,
                        Type = MenuItem.MenuType.CatId,
                        Id = parent.cat_id
                    });
                }
                var c = new MenuItem()
                {
                    Title = current.cat_name,
                    Type = MenuItem.MenuType.CatId,
                    Id = catId
                };
                c.Color = UIColor.LightGray;
                source.Items.Add(c);
                foreach (var item in cats)
                {
                    source.Items.Add(new MenuItem()
                    {
                        Title = item.cat_name,
                        Type = MenuItem.MenuType.CatId,
                        Id = item.cat_id
                    });
                }
            }
            MenuTableViewReference.Source = source;
            MenuTableViewReference.ReloadData();
        }

        public void RefreshMenu()
        {

            var source = new MenuTableViewSource();
            source.Items = new System.Collections.Generic.List<MenuItem>();

            if (Customer == null)
            {
                source.Items.Add(new MenuItem()
                {
                    Title = "ورود ",
                    Icon = UIImage.FromBundle("ic_log_in_green_24dp.png"),
                    Type = MenuItem.MenuType.Login
                });
            }
            else
            {
                if (Customer != null && Customer.FirstName != null && Customer.LastName != null)
                {
                    source.Items.Add(new MenuItem()
                    {
                        Title = Customer.FirstName + " " + Customer.LastName,
                        Icon = UIImage.FromBundle("ic_person_gray_24dp"),
                        Type = MenuItem.MenuType.Profile
                    });
                }
                else
                {
                    source.Items.Add(new MenuItem()
                    {
                        Title = Customer.Phone,
                        Icon = UIImage.FromBundle("ic_person_gray_24dp"),
                        Type = MenuItem.MenuType.Profile
                    });
                }
            }
            source.Items.Add(new MenuItem()
            {
                Title = "صفحه اول",
                Type = MenuItem.MenuType.FirstPage
            });
            source.Items.Add(new MenuItem()
            {
                Title = "دسته بندی کالا ",
                Icon = UIImage.FromBundle("ic_list_orange_24dp"),
                Type = MenuItem.MenuType.Products
            });

            if (Customer != null)
                source.Items.Add(new MenuItem()
                {
                    Title = " لیست من",
                    Type = MenuItem.MenuType.Favorits
                });

            if (Customer != null)
                source.Items.Add(new MenuItem()
                {
                    Title = " سفارشات قبلی",
                    Type = MenuItem.MenuType.Orders
                });

            if (DefaultStore == null)
                source.Items.Add(new MenuItem()
                {
                    Title = " فروشگاه من",
                    Type = MenuItem.MenuType.Stores
                });
            else
                source.Items.Add(new MenuItem()
                {
                    Title = " فروشگاه من" + "(" + DefaultStore.store_name + ")",
                    Type = MenuItem.MenuType.Stores
                });
            MenuTableViewReference.Source = source;
            MenuTableViewReference.ReloadData();
        }

        public void CloseMenu()
        {
            (UIApplication.SharedApplication.Delegate as AppDelegate).RootViewController.SidebarController.CloseMenu();
        }
        public void OpenMenu()
        {
            (UIApplication.SharedApplication.Delegate as AppDelegate).RootViewController.SidebarController.OpenMenu();
        }
        public void SelectMenuItem(int index)
        {
            var items = (MenuTableViewReference.Source as MenuTableViewSource).Items;
            switch (items[index].Type)
            {
                case MenuItem.MenuType.FirstPage:
                    PushViewController(new FirstPageViewController());
                    CloseMenu();
                    break;
                case MenuItem.MenuType.Favorits:
                    PushViewController(new FavoritsViewController());
                    CloseMenu();
                    break;
                case MenuItem.MenuType.Login:
                    PushViewController(new LoginViewController());
                    CloseMenu();
                    break;
                case MenuItem.MenuType.Profile:
                    PushViewController(new ProfileViewController());
                    CloseMenu();
                    break;
                case MenuItem.MenuType.Orders:
                    PushViewController(new OrdersStatusViewController());
                    CloseMenu();
                    break;
                case MenuItem.MenuType.Products:
                    if (DefaultStore != null)
                    {
                        //await RefreshMenu ("0");
                        PushViewController(new ProductsViewController(), true);
                    }
                    else
                    {
                        PushViewController(new StoresViewController());
                        CloseMenu();
                    }
                    break;
                case MenuItem.MenuType.Stores:
                    PushViewController(new StoresViewController());
                    CloseMenu();
                    break;
                case MenuItem.MenuType.CatId:
                    var view = (GetVisibleViewController() as ProductsViewController);
                    if (items.Count > index)
                    {
                        if (items[index] != null)
                        {
                            if (items[index].Id == view.GroupId)
                            {
                                CloseMenu();
                                break;
                            }
                            //await RefreshMenu (items [index].Id);
                            var v = new ProductsViewController();
                            v.GroupId = items[index].Id;
                            PushViewController(v, true);
                        }
                    }
                    break;
                case MenuItem.MenuType.MainMenu:
                    RefreshMenu();
                    break;
                default:
                    break;
            }
        }

        public async Task LogOutAsync()
        {
            await AnatoliUserManager.LogoutAsync();
            Customer = null;
            User = null;
            RefreshMenu();
        }

        public UIBarButtonItem[] CreateToolbarItems()
        {
            var b1 = CreateToolBarButton(UIImage.FromFile("ic_list_orange_24dp"), () => { PushViewController(new FirstPageViewController()); });
            if (GetVisibleViewController().GetType() != typeof(FirstPageViewController))
                b1.TintColor = UIColor.DarkGray;
            var b2 = CreateToolBarButton(UIImage.FromFile("ic_list_orange_24dp"), () => { PushViewController(new ProductsViewController()); });
            if (GetVisibleViewController().GetType() != typeof(ProductsViewController))
                b2.TintColor = UIColor.DarkGray;
            UIBarButtonItem b3;
            if (GetVisibleViewController().GetType() != typeof(ShoppingCardViewController))
                b3 = CreateBasketButton(false);
            else
                b3 = CreateBasketButton(true);
            var b4 = CreateToolBarButton(UIImage.FromFile("ic_mylist_orange_24dp"), () => { PushViewController(new FavoritsViewController()); });
            if (GetVisibleViewController().GetType() != typeof(FavoritsViewController))
                b4.TintColor = UIColor.DarkGray;
            var b5 = CreateToolBarButton(UIImage.FromFile("ic_reorder_white_24dp"), () => { (UIApplication.SharedApplication.Delegate as AppDelegate).RootViewController.SidebarController.ToggleMenu(); });
            b5.TintColor = UIColor.DarkGray;
            var space = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) { Width = 50 };
            return new UIBarButtonItem[] { b1, space, b2, space, b3, space, b4, space, b5 };
        }
        public UIBarButtonItem CreateToolBarButton(UIImage image, Action delCommand)
        {
            return new UIBarButtonItem(image.Scale(new CoreGraphics.CGSize(26, 26))
                , UIBarButtonItemStyle.Plain
                , (sender, args) =>
                {
                    delCommand.Invoke();
                });
        }

        public UIBarButtonItem CreateBasketButton(bool enabled = false)
        {

            var b = new UIBarButtonItem(UIImage.FromBundle("ic_shoppingcard_on_white_24dp").Scale(new CoreGraphics.CGSize(26, 26))
              , UIBarButtonItemStyle.Plain
              , (sender, args) =>
              {
                  if (Customer == null)
                  {
                      var loginAlert = UIAlertController.Create("خطا", "لطفا ابتدا وارد حساب کاربری خود شوید", UIAlertControllerStyle.Alert);
                      loginAlert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default,
                          delegate
                          {
                              PushViewController(new LoginViewController());
                          }
                      ));
                      loginAlert.AddAction(UIAlertAction.Create("بی خیال", UIAlertActionStyle.Cancel, null));
                      PresentViewController(loginAlert);
                  }
                  else if (DefaultStore == null)
                  {
                      var storeAlert = UIAlertController.Create("خطا", "لطفا ابتدا فروشگاه پیش فرض را انتخاب نمایید", UIAlertControllerStyle.Alert);
                      storeAlert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default,
                          delegate
                          {
                              PushViewController(new StoresViewController());
                          }
                      ));
                      storeAlert.AddAction(UIAlertAction.Create("بی خیال", UIAlertActionStyle.Cancel, null));
                      PresentViewController(storeAlert);
                  }
                  else
                      AnatoliApp.GetInstance().PushViewController(new ShoppingCardViewController());
                  CloseMenu();
              });
            if (!enabled)
                b.TintColor = UIColor.DarkGray;
            return b;
            
            //UIView basketView = new UIView(new CGRect(0, 0, 26, 26));

            //var button = new UIButton(new CGRect(0, 0, 26, 26));
            //if (enabled)
            //    button.TintColor = UIColor.Blue;
            //else
            //    button.TintColor = UIColor.DarkGray;
            //button.SetBackgroundImage(UIImage.FromBundle("ic_shoppingcard_on_white_24dp").Scale(new CoreGraphics.CGSize(26, 26)), UIControlState.Normal);
            //button.TouchUpInside += (object sender, EventArgs e) =>
            //{
            //    if (Customer == null)
            //    {
            //        var loginAlert = UIAlertController.Create("خطا", "لطفا ابتدا وارد حساب کاربری خود شوید", UIAlertControllerStyle.Alert);
            //        loginAlert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default,
            //            delegate
            //            {
            //                PushViewController(new LoginViewController());
            //            }
            //        ));
            //        loginAlert.AddAction(UIAlertAction.Create("بی خیال", UIAlertActionStyle.Cancel, null));
            //        PresentViewController(loginAlert);
            //    }
            //    else if (DefaultStore == null)
            //    {
            //        var storeAlert = UIAlertController.Create("خطا", "لطفا ابتدا فروشگاه پیش فرض را انتخاب نمایید", UIAlertControllerStyle.Alert);
            //        storeAlert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default,
            //            delegate
            //            {
            //                PushViewController(new StoresViewController());
            //            }
            //        ));
            //        storeAlert.AddAction(UIAlertAction.Create("بی خیال", UIAlertActionStyle.Cancel, null));
            //        PresentViewController(storeAlert);
            //    }
            //    else
            //        AnatoliApp.GetInstance().PushViewController(new ShoppingCardViewController());
            //    CloseMenu();
            //};

            //_counterLabel = new UILabel(new CGRect(18, -3, 12, 12));
            //_counterLabel.TextAlignment = UITextAlignment.Center;
            //_counterLabel.TextColor = UIColor.White;
            //_counterLabel.Layer.MasksToBounds = true;
            //_counterLabel.Layer.CornerRadius = 6;
            //_counterLabel.BackgroundColor = UIColor.Red;
            //_counterLabel.Font = UIFont.FromName("IRAN", 9);
            //_counterLabel.Text = ShoppingCardItemsCount.ToString();

            ////			_priceLabel = new UILabel(new CGRect(-7,20,40,12));
            ////			_priceLabel.TextAlignment = UITextAlignment.Center;
            ////			_priceLabel.TextColor = UIColor.White;
            ////			_counterLabel.Layer.MasksToBounds = true;
            ////			_counterLabel.Layer.CornerRadius = 6;
            ////			_priceLabel.BackgroundColor = UIColor.Blue;
            ////			_priceLabel.Font = UIFont.FromName ("IRAN", 8);
            ////			_priceLabel.Text = ShoppingCardTotalPrice.ToCurrency () + " تومان";


            //ShoppingCardManager.ItemChanged -= UpdateBasketView;
            //ShoppingCardManager.ItemChanged += UpdateBasketView;

            //ShoppingCardManager.ItemsCleared -= ResetBasketView;
            //ShoppingCardManager.ItemsCleared += ResetBasketView;

            ////			basketView.AddSubviews (button, _counterLabel,_priceLabel);
            //basketView.AddSubviews(button, _counterLabel);
            //var barButton = new UIBarButtonItem(basketView);
            //return barButton;
        }

        async void UpdateBasketView(ProductModel item)
        {
            var info = await ShoppingCardManager.GetInfoAsync();
            if (info != null)
            {
                ShoppingCardItemsCount = info.items_count;
                ShoppingCardTotalPrice = info.total_price;
                _counterLabel.Text = ShoppingCardItemsCount.ToString();
            }

            //_priceLabel.Text = ShoppingCardTotalPrice.ToCurrency () + " تومان";
        }
        void ResetBasketView()
        {
            ShoppingCardItemsCount = 0;
            ShoppingCardTotalPrice = 0;
            _counterLabel.Text = "0";
            //_priceLabel.Text = "0 تومان";
        }

        public async Task LoginAsync()
        {
            Customer = await CustomerManager.ReadCustomerAsync();
            User = await AnatoliUserManager.ReadUserInfoAsync();
            RefreshMenu();
        }
    }

    public static class Extensions
    {
        public static UIColor FromHex(this UIColor color, int hexValue)
        {
            return UIColor.FromRGB(
                (((float)((hexValue & 0xFF0000) >> 16)) / 255.0f),
                (((float)((hexValue & 0xFF00) >> 8)) / 255.0f),
                (((float)(hexValue & 0xFF)) / 255.0f)
            );
        }
    }


}
