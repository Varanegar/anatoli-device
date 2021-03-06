﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AnatoliAndroid;
using Anatoli.App.Model.AnatoliUser;
using AnatoliAndroid.ListAdapters;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using AnatoliAndroid.Fragments;
using Anatoli.App.Manager;
using Android.Locations;
using Android.Views.InputMethods;
using Anatoli.Framework.AnatoliBase;
using Anatoli.App.Model;
using System.Threading.Tasks;
using Anatoli.App.Model.Product;
using Android.Views.Animations;
using Anatoli.App.Model.Store;
using Activity = Android.Support.V7.App.AppCompatActivity;

namespace AnatoliAndroid.Activities
{
    class AnatoliApp
    {
        private static AnatoliApp _instance;
        private Activity _context;
        public LocationManager LocationManager;
        public AnatoliUserModel AnatoliUser { get; private set; }
        public List<DrawerItemType> MenuItems;
        ListView _drawerListView;
        public ListView DrawerListView { get { return _drawerListView; } }
        private static LinkedList<StackItem> _list;
        int _backToExit = 0;
        public bool ExitApp { get { return (_backToExit >= 2) ? true : false; } }
        Toolbar ToolBar;
        public Android.Support.V4.Widget.DrawerLayout DrawerLayout;
        RelativeLayout _searchBarLayout;
        RelativeLayout _toolBarLayout;
        ImageButton _shoppingCardImageButton;
        ImageButton _searchImageButton;
        ImageButton _searchBarImageButton;
        ImageButton _searchButtonImageButton;
        ImageButton _toolBarImageButton;
        ImageButton _menuIconImageButton;
        TextView _shoppingCardTextView;
        TextView _shoppingPriceTextView;
        public CustomerViewModel Customer { get; set; }
        double _price;
        public StoreModel DefaultStore { get; set; }

        void SetTotalPrice(double price)
        {
            _price = price;
            if ((int)_price != 0)
                _shoppingPriceTextView.Visibility = ViewStates.Visible;
            else
                _shoppingPriceTextView.Visibility = ViewStates.Invisible;
            _shoppingPriceTextView.Text = _price.ToCurrency() + " تومان";
            Animation anim = AnimationUtils.LoadAnimation(Activity, Resource.Animation.abc_shrink_fade_out_from_bottom);
            _shoppingPriceTextView.StartAnimation(anim);
            _shoppingCardTextView.StartAnimation(anim);
        }
        public void HideMenuIcon()
        {
            _menuIconImageButton.Visibility = ViewStates.Gone;
        }

        public void ShowMenuIcon()
        {
            _menuIconImageButton.Visibility = ViewStates.Visible;
        }

        public void HideSearchIcon()
        {
            _searchImageButton.Visibility = ViewStates.Gone;
        }

        public void ShowSearchIcon()
        {
            _searchImageButton.Visibility = ViewStates.Visible;
        }
        public delegate void ProcessMenu();
        public ProcessMenu MenuClicked;
        void OnMenuClick()
        {
            if (MenuClicked != null)
            {
                MenuClicked();
            }
        }
        TextView _toolBarTextView;
        public void SetToolbarTitle(string title)
        {
            _toolBarTextView.Text = title;
        }
        AutoCompleteTextView _searchEditText;
        ArrayAdapter _autoCompleteAdapter;
        string[] _autoCompleteOptions;
        bool _searchBar = false;
        public bool SearchBarEnabled { get { return _searchBar; } }
        public void CloseSearchBar()
        {
            _searchBarLayout.Visibility = ViewStates.Gone;
            _toolBarLayout.Visibility = ViewStates.Visible;
            _searchEditText.Text = "";
            _searchBar = false;
        }
        public void OpenSearchBar()
        {
            _toolBarLayout.Visibility = ViewStates.Gone;
            _searchBarLayout.Visibility = ViewStates.Visible;
            _searchEditText.RequestFocus();
            _searchBar = true;
        }
        public void ShowKeyboard(View pView)
        {
            pView.RequestFocus();
            InputMethodManager inputMethodManager = _context.GetSystemService(Context.InputMethodService) as InputMethodManager;
            inputMethodManager.ShowSoftInput(pView, ShowFlags.Forced);
            inputMethodManager.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.ImplicitOnly);
        }
        public void HideKeyboard(View pView)
        {
            InputMethodManager inputMethodManager = _context.GetSystemService(Context.InputMethodService) as InputMethodManager;
            inputMethodManager.HideSoftInputFromWindow(pView.WindowToken, HideSoftInputFlags.None);
        }
        public void CreateToolbar()
        {
            _searchEditText = ToolBar.FindViewById<AutoCompleteTextView>(Resource.Id.searchEditText);
            _searchEditText.FocusChange += (s, e) =>
            {
                if (e.HasFocus)
                    ShowKeyboard(_searchEditText);
                else
                    HideKeyboard(_searchEditText);
            };
            _searchEditText.EditorAction += (s, e) =>
            {
                if (e.ActionId == ImeAction.Done)
                {
                    Search(_searchEditText.Text);
                    CloseSearchBar();
                }
            };
            _toolBarTextView = ToolBar.FindViewById<TextView>(Resource.Id.toolbarTextView);
            _shoppingCardTextView = ToolBar.FindViewById<TextView>(Resource.Id.shoppingCardTextView);
            _shoppingPriceTextView = ToolBar.FindViewById<TextView>(Resource.Id.shoppingPriceTextView);
            _searchBarLayout = ToolBar.FindViewById<RelativeLayout>(Resource.Id.searchRelativeLayout);
            _toolBarLayout = ToolBar.FindViewById<RelativeLayout>(Resource.Id.toolbarRelativeLayout);
            CloseSearchBar();

            _toolBarImageButton = ToolBar.FindViewById<ImageButton>(Resource.Id.toolbarImageButton);
            _toolBarImageButton.Click += toolbarImageButton_Click;
            _toolBarTextView.Click += toolbarImageButton_Click;

            _searchImageButton = ToolBar.FindViewById<ImageButton>(Resource.Id.searchImageButton);
            _searchImageButton.Click += searchImageButton_Click;

            _searchBarImageButton = ToolBar.FindViewById<ImageButton>(Resource.Id.searchbarImageButton);
            _searchBarImageButton.Click += _searchBarImageButton_Click;

            _searchButtonImageButton = ToolBar.FindViewById<ImageButton>(Resource.Id.searchButtonImageButton);
            _searchButtonImageButton.Click += (s, e) =>
            {
                DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
                Search(_searchEditText.Text);
            };


            _autoCompleteOptions = new String[] { "نوشیدنی", "لبنیات", "پروتئینی", "خواربار", "روغن", "پنیر", "شیر", "ماست", "کره", "دوغ", "گوشت" };
            _autoCompleteAdapter = new ArrayAdapter(_context, Resource.Layout.AutoCompleteDropDownLayout, _autoCompleteOptions);

            _searchEditText.Adapter = _autoCompleteAdapter;
            _searchEditText.TextChanged += (s, e) =>
           {
               if (e.AfterCount >= 3)
               {
                   if (GetInstance().GetCurrentFragmentType() == typeof(ProductsListFragment) ||
                   GetInstance().GetCurrentFragmentType() == typeof(FirstFragment))
                   {
                       var options = (ProductManager.GetSuggests(_searchEditText.Text, 20));
                       if (options != null)
                       {
                           _autoCompleteOptions = options.ToArray();
                           _autoCompleteAdapter = new ArrayAdapter(_context, Resource.Layout.AutoCompleteDropDownLayout, _autoCompleteOptions);
                           _searchEditText.Adapter = _autoCompleteAdapter;
                           _searchEditText.Invalidate();
                       }

                   }
               }
           };
            _searchEditText.ItemClick += (s, e) =>
            {
                HideKeyboard(_searchEditText);
                Search(_searchEditText.Text);
            };
            _shoppingCardImageButton = ToolBar.FindViewById<ImageButton>(Resource.Id.shoppingCardImageButton);
            var shoppingbarRelativeLayout = ToolBar.FindViewById<RelativeLayout>(Resource.Id.shoppingbarRelativeLayout);
            shoppingbarRelativeLayout.Click += shoppingbarRelativeLayout_Click;
            _shoppingCardImageButton.Click += shoppingbarRelativeLayout_Click;
            _menuIconImageButton = ToolBar.FindViewById<ImageButton>(Resource.Id.menuImageButton);
            _menuIconImageButton.Click += (s, e) => { OnMenuClick(); };

            ShoppingCardManager.ItemChanged += delegate
            {
                var cardInfoChange = ShoppingCardManager.GetInfo();
                SetTotalPrice(cardInfoChange.TotalPrice);
                _shoppingCardTextView.Text = (cardInfoChange.Qty).ToString();
            };
            ShoppingCardManager.ItemsCleared += delegate
            {
                SetTotalPrice(0);
                _shoppingCardTextView.Text = "0";
            };
        }

        public void UpdateBasketIcon()
        {

            var cardInfoChange = ShoppingCardManager.GetInfo();
            SetTotalPrice(cardInfoChange.TotalPrice);
            _shoppingCardTextView.Text = (cardInfoChange.Qty).ToString();
        }

        void shoppingbarRelativeLayout_Click(object sender, EventArgs e)
        {
            _shoppingCardImageButton.Enabled = false;
            if (AnatoliApp.GetInstance().AnatoliUser == null)
            {
                Toast.MakeText(AnatoliApp.GetInstance().Activity, Resource.String.PleaseLogin, ToastLength.Short).Show();
                LoginFragment login = new LoginFragment();
                var transaction = AnatoliApp.GetInstance().Activity.FragmentManager.BeginTransaction();
                login.LoginSuceeded += () => { PushFragment(new ShoppingCardFragment(), "shoppingCard_fragment"); };
                login.Show(transaction, "login_fragment");
                return;
            }
            PushFragment(new ShoppingCardFragment(), "shoppingCard_fragment");
            DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
            _shoppingCardImageButton.Enabled = true;
        }

        void Search(string value)
        {
            value = value.Trim();
            if (String.IsNullOrEmpty(value))
                return;
            if (GetInstance().GetCurrentFragmentType() == typeof(ProductsListFragment))
            {
                var p = new ProductsListFragment();
                p.Search(ProductManager.Search(value, AnatoliApp.GetInstance().DefaultStore.UniqueId.ToString()), value);
                PushFragment(p, "products_fragment", true);
            }
            if (AnatoliApp.GetInstance().GetCurrentFragmentType() == typeof(FirstFragment) ||
                AnatoliApp.GetInstance().GetCurrentFragmentType() == typeof(ProductDetailFragment))
            {
                var p = new ProductsListFragment();
                p.Search(ProductManager.Search(value, AnatoliApp.GetInstance().DefaultStore.UniqueId.ToString()), value);
                PushFragment(p, "products_fragment");
            }

        }
        void _searchBarImageButton_Click(object sender, EventArgs e)
        {
            CloseSearchBar();
            DrawerLayout.CloseDrawer(GetInstance().DrawerListView);
        }

        void searchImageButton_Click(object sender, EventArgs e)
        {
            DrawerLayout.CloseDrawer(GetInstance().DrawerListView);
            if (!_searchBar)
            {
                OpenSearchBar();
            }
        }

        void toolbarImageButton_Click(object sender, EventArgs e)
        {
            if (DrawerLayout.IsDrawerOpen(GetInstance().DrawerListView))
            {
                DrawerLayout.CloseDrawer(GetInstance().DrawerListView);
            }
            else
            {
                DrawerLayout.OpenDrawer(GetInstance().DrawerListView);
            }
        }

        public Type GetCurrentFragmentType()
        {
            try
            {
                return _list.Last().FragmentType;
            }
            catch (Exception)
            {
                return null;
            }
        }
        AnatoliFragment _currentFragment;
        public Activity Activity { get { return _context; } }
        public static AnatoliApp GetInstance()
        {
            if (_instance == null)
                throw new NullReferenceException();
            return _instance;
        }
        public static void Initialize(Activity activity, AnatoliUserModel user, ListView drawerListView, Toolbar toolbar)
        {
            _instance = new AnatoliApp(activity, user, drawerListView, toolbar);
            _list = new LinkedList<StackItem>();
        }
        private AnatoliApp()
        {

        }
        public int CurrentVersionCode { get; private set; }
        public int OldVersionCode { get; private set; }
        String PREFS_NAME = "MyPrefsFile";
        String PREF_VERSION_CODE_KEY = "version_code";
        int DOESNT_EXIST = -1;
        private AnatoliApp(Activity activity, AnatoliUserModel user, ListView drawerListView, Toolbar toolbar)
        {
            _context = activity;
            AnatoliUser = user;
            _drawerListView = drawerListView;
            _drawerListView.ItemClick += _drawerListView_ItemClick;
            ToolBar = toolbar;
            CreateToolbar();


            try
            {
                CurrentVersionCode = _context.PackageManager.GetPackageInfo(_context.PackageName, 0).VersionCode;
                var prefs = _context.GetSharedPreferences(PREFS_NAME, FileCreationMode.Private);
                OldVersionCode = prefs.GetInt(PREF_VERSION_CODE_KEY, DOESNT_EXIST);
                if (CurrentVersionCode == OldVersionCode)
                {
                    // This is just a normal run
                }
                else if (OldVersionCode == DOESNT_EXIST)
                {
                    // TODO This is a new install (or the user cleared the shared preferences)
                    AnatoliClient.GetInstance().DbClient.Create();
                    prefs.Edit().PutInt(PREF_VERSION_CODE_KEY, CurrentVersionCode).Commit();
                }
                else if (CurrentVersionCode > OldVersionCode)
                {
                    // TODO This is an upgrade
                    AnatoliClient.GetInstance().DbClient.Upgrade(CurrentVersionCode, OldVersionCode);
                    prefs.Edit().PutInt(PREF_VERSION_CODE_KEY, CurrentVersionCode).Commit();
                }
                prefs.Edit().PutInt(PREF_VERSION_CODE_KEY, CurrentVersionCode).Commit();
            }
            catch
            {
            }

        }
        void _drawerListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var selectedItem = GetInstance().MenuItems[e.Position];
            GetInstance().DrawerListView.SetItemChecked(e.Position, true);
            if (selectedItem.GetType() == typeof(DrawerMainItem))
            {
                switch (selectedItem.ItemId)
                {
                    case DrawerMainItem.DrawerMainItems.ProductCategries:
                        if (DefaultStore == null)
                        {
                            Toast.MakeText(Activity, "لطفا ابتدا فروشگاه مورد نظر را انتخاب نمایید", ToastLength.Short).Show();
                            DrawerLayout.CloseDrawer(GetInstance().DrawerListView);
                            StoresListFragment s = new StoresListFragment();
                            s.SetQuery(StoreManager.GetAllQueryString());
                            GetInstance().PushFragment(s, "stores_fragment");
                            return;
                        }
                        else
                        {
                            var p = new ProductsListFragment();
                            p.SetQuery(ProductManager.GetAll(AnatoliApp.GetInstance().DefaultStore.UniqueId));
                            PushFragment(p, "products_fragment", true);
                            RefreshMenuItems(null);
                        }
                        break;
                    case DrawerMainItem.DrawerMainItems.ShoppingCard:
                        if (GetInstance().AnatoliUser == null)
                        {
                            Toast.MakeText(GetInstance().Activity, Resource.String.PleaseLogin, ToastLength.Short).Show();
                            LoginFragment login = new LoginFragment();
                            login.LoginSuceeded += () => { GetInstance().PushFragment(new ShoppingCardFragment(), "shoppingCard_fragment"); };
                            var tt = GetInstance().Activity.FragmentManager.BeginTransaction();
                            login.Show(tt, "login_fragment");
                            break;
                        }
                        DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
                        break;
                    case DrawerMainItem.DrawerMainItems.StoresList:
                        DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
                        var stores = new StoresListFragment();
                        stores.SetQuery(StoreManager.GetAllQueryString());
                        AnatoliApp.GetInstance().PushFragment(stores, "stores_fragment");
                        break;
                    case DrawerMainItem.DrawerMainItems.FirstPage:
                        DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
                        AnatoliApp.GetInstance().PushFragment(new FirstFragment(), "first_fragment");
                        break;
                    case DrawerMainItem.DrawerMainItems.Login:
                        DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
                        var transaction = Activity.FragmentManager.BeginTransaction();
                        var loginFragment = new LoginFragment();
                        loginFragment.LoginSuceeded += () =>
                        {
                            if (StoreManager.GetDefault() == null)
                            {
                                Toast.MakeText(Activity, "لطفا ابتدا فروشگاه مورد نظر را انتخاب نمایید", ToastLength.Short).Show();
                                DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
                                var s = new StoresListFragment();
                                s.SetQuery(StoreManager.GetAllQueryString());
                                AnatoliApp.GetInstance().PushFragment(s, "stores_fragment");
                                return;
                            }
                            var firstFragment = new FirstFragment();
                            AnatoliApp.GetInstance().PushFragment(firstFragment, "first_fragment");
                        };
                        loginFragment.Show(transaction, "shipping_dialog");
                        break;
                    case DrawerMainItem.DrawerMainItems.MainMenu:
                        AnatoliApp.GetInstance().RefreshMenuItems();
                        break;
                    case DrawerMainItem.DrawerMainItems.Favorits:
                        DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
                        var f = new FavoritsListFragment();
                        f.SetQuery(ProductManager.GetFavoritsQueryString());
                        AnatoliApp.GetInstance().PushFragment(f, "favorits_fragment");
                        break;
                    case DrawerMainItem.DrawerMainItems.Profile:
                        DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
                        var profileF = new ProfileFragment();
                        var tr = _context.FragmentManager.BeginTransaction();
                        profileF.Show(tr, "profile_fragment");
                        break;
                    case DrawerMainItem.DrawerMainItems.About:
                        DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
                        var aboutF = new AboutDialog();
                        tr = _context.FragmentManager.BeginTransaction();
                        aboutF.Show(tr, "about_fragment");
                        break;
                    case DrawerMainItem.DrawerMainItems.Messages:
                        DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
                        var m = new MessagesListFragment();
                        m.SetQuery(MessageManager.GetAllQueryString());
                        AnatoliApp.GetInstance().PushFragment(m, "messages_fragment");
                        break;
                    case DrawerMainItem.DrawerMainItems.Orders:
                        DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
                        var o = new OrdersListFragment();
                        o.SetQuery(PurchaseOrderManager.GetOrderQueryString());
                        AnatoliApp.GetInstance().PushFragment(o, "orders_fragment");
                        break;
                    case DrawerMainItem.DrawerMainItems.Logout:
                        DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
                        var result = Logout();
                        if (result)
                        {
                            AnatoliApp.GetInstance().PushFragment(new FirstFragment(), "first_fragment");
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                var p = new ProductsListFragment();
                p.SetCatId(Guid.Parse(selectedItem.ItemId));
                PushFragment(p, "products_fragment", true);
                AnatoliApp.GetInstance()._toolBarTextView.Text = selectedItem.Name;
                DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
                if ((selectedItem as DrawerProductGroupItem).ItemType == DrawerProductGroupItem.ItemTypes.Leaf)
                {
                    DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
                }
            }
        }
        internal async Task<CustomerViewModel> RefreshCutomerProfile(bool cancelable = false)
        {
            if (AnatoliClient.GetInstance().WebClient.IsOnline() && AnatoliApp.GetInstance().AnatoliUser != null)
            {
                AlertDialog.Builder errDialog = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                Android.App.ProgressDialog pDialog = new Android.App.ProgressDialog(_context);
                pDialog.SetTitle(_context.Resources.GetText(Resource.String.Updating));
                pDialog.SetMessage(_context.Resources.GetText(Resource.String.PleaseWait));
                System.Threading.CancellationTokenSource cancellationTokenSource = new System.Threading.CancellationTokenSource();
                if (cancelable)
                {
                    pDialog.SetButton("بی خیال", (s, e) => { cancellationTokenSource.Cancel(); });
                    pDialog.CancelEvent += (s, e) => { cancellationTokenSource.Cancel(); };
                    pDialog.Show();
                }
                try
                {
                    var c = await CustomerManager.DownloadCustomerAsync(AnatoliApp.GetInstance().AnatoliUser, cancellationTokenSource);
                    Customer = c;
                    RefreshMenuItems();
                    pDialog.Dismiss();
                    if (c.IsValid)
                    {
                        await CustomerManager.SaveCustomerAsync(c);
                    }
                    return c;
                }
                catch (Exception e)
                {
                    e.SendTrace();
                    if (cancelable && e.GetType() == typeof(TaskCanceledException))
                    {
                        errDialog.SetMessage(Resource.String.ErrorOccured);
                        errDialog.SetPositiveButton(Resource.String.Ok, (s2, e2) => { });
                        errDialog.Show();
                    }
                }
            }
            return null;
        }

        internal async Task SyncDatabase()
        {
            var latestUpdateTime = SyncManager.GetLog(SyncManager.UpdateCompleted);

            if ((DateTime.Now - latestUpdateTime).TotalDays > 3)
            {
                if (!AnatoliClient.GetInstance().WebClient.IsOnline())
                {
                    AlertDialog.Builder alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                    alert.SetMessage(Resource.String.PleaseConnectToInternet);
                    alert.SetCancelable(false);
                    alert.SetPositiveButton(Resource.String.Ok, async delegate
                    {
                        await SyncDatabase();
                    });
                    alert.SetNegativeButton("تنظیمات", delegate
                    {
                        Intent intent = new Intent(Android.Provider.Settings.ActionSettings);
                        AnatoliApp.GetInstance().Activity.StartActivity(intent);
                    });
                    alert.Show();
                    return;
                }
                Android.App.ProgressDialog pDialog = new Android.App.ProgressDialog(_context);
                pDialog.SetCancelable(false);
                SyncManager.ProgressChanged += (status, step) =>
                {
                    pDialog.SetMessage(status);
                };

                try
                {
                    pDialog.Show();
                    await SyncManager.SyncDatabase();
                }
                catch (System.Net.WebException)
                {
                    AlertDialog.Builder alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                    alert.SetCancelable(false);
                    alert.SetMessage("لطفا دستگاه خود را به منظور بروزرسانی اطلاعات به اینترنت متصل نمایید");
                    alert.SetPositiveButton(Resource.String.Ok, async delegate
                    {
                        await SyncDatabase();
                    });
                    alert.SetNegativeButton("تنظیمات", delegate
                    {
                        Intent intent = new Intent(Android.Provider.Settings.ActionSettings);
                        AnatoliApp.GetInstance().Activity.StartActivity(intent);
                    });
                    alert.Show();
                    return;
                }
                catch (Exception e)
                {
                    e.SendTrace();
                    AlertDialog.Builder alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                    alert.SetCancelable(false);
                    alert.SetMessage("بروزرسانی اطلاعات با خطا مواجه شد");
                    alert.SetPositiveButton("دوباره تلاش کن", async delegate
                    {
                        await SyncDatabase();
                    });
                    alert.Show();
                    return;
                }
                finally
                {
                    pDialog.Dismiss();
                }
            }
            else if ((DateTime.Now - latestUpdateTime).TotalMinutes > 20)
            {
                if (!AnatoliClient.GetInstance().WebClient.IsOnline())
                {
                    Toast.MakeText(Activity, "لطفا دستگاه خود را به منظور بروزرسانی اطلاعات به اینترنت متصل نمایید", ToastLength.Short).Show();
                    return;
                }

                Activity.StartService(new Intent(Activity, typeof(SyncDataBaseService)));
            }
        }
        internal void PushFragment(AnatoliFragment fragment, string tag, Tuple<string, string> parameter, bool allowingStateLoss = false)
        {
            if (fragment == null)
            {
                throw new ArgumentNullException();
            }
            Bundle bundle = new Bundle();
            bundle.PutString(parameter.Item1, parameter.Item2);
            fragment.Arguments = bundle;
            PushFragment(fragment, tag, allowingStateLoss);
        }
        public void PushFragment(AnatoliFragment fragment, string tag, bool force = false, bool allowingStateLoss = false)
        {
            if (_list.Count > 0)
            {
                if ((fragment.GetType() == _list.Last.Value.FragmentType) && !force)
                {
                    return;
                }
            }

            var transaction = _context.SupportFragmentManager.BeginTransaction();
            if (fragment == null)
            {
                throw new ArgumentNullException();
            }
            transaction.SetCustomAnimations(Android.Resource.Animation.SlideInLeft, Android.Resource.Animation.SlideOutRight);
            transaction.Replace(Resource.Id.content_frame, fragment, tag);
            if (allowingStateLoss)
                transaction.CommitAllowingStateLoss();
            else
                transaction.Commit();
            _toolBarTextView.Text = fragment.Title;
            foreach (var item in _list)
            {
                if (item.FragmentType != typeof(ProductsListFragment))
                {
                    if (item.FragmentType == fragment.GetType())
                    {
                        if (item != _list.First())
                        {
                            _list.Remove(item);
                            break;
                        }
                    }
                }

            }
            _list.AddLast(new StackItem(tag, fragment));
            _currentFragment = fragment;
            if (_currentFragment.GetType() != typeof(ProductsListFragment))
            {
                RefreshMenuItems();
            }
            _backToExit = 0;
        }
        public bool BackFragment()
        {
            var transaction = _context.SupportFragmentManager.BeginTransaction();
            try
            {
                if (_list.Count <= 1)
                {
                    _backToExit++;
                    return false;
                }
                if (_list.Last().FragmentType == typeof(FirstFragment))
                {
                    _backToExit++;
                    return false;
                }
                else
                {
                    _list.RemoveLast();
                    var stackItem = _list.Last();
                    if (stackItem.FragmentType == typeof(LoginFragment) && AnatoliUser != null)
                    {
                        _list.RemoveLast();
                        stackItem = _list.Last();
                    }
                    if (stackItem.FragmentType == typeof(ProfileFragment) && AnatoliUser == null)
                    {
                        _list.RemoveLast();
                        stackItem = _list.Last();
                    }
                    var fragment = stackItem.Fragment;
                    transaction.SetCustomAnimations(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                    transaction.Replace(Resource.Id.content_frame, fragment as AnatoliFragment, stackItem.FragmentName);
                    transaction.Commit();
                    _toolBarTextView.Text = (fragment as AnatoliFragment).Title;
                    if (fragment.GetType() != typeof(ProductsListFragment))
                    {
                        RefreshMenuItems();
                    }
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        public void RefreshMenuItems(Guid? catId)
        {
            List<ProductGroupModel> temp;
            if (catId == null)
                temp = ProductGroupManager.GetFirstLevel();
            else
                temp = ProductGroupManager.GetProductGroups((Guid)catId);
            if (temp == null || temp.Count == 0)
            {
                return;
            }
            var categories = new List<DrawerItemType>();
            categories.Add(new DrawerMainItem(DrawerMainItem.DrawerMainItems.MainMenu, _context.Resources.GetText(Resource.String.MainMenu)));
            categories.Add(new DrawerMainItem(DrawerMainItem.DrawerMainItems.ProductCategries, _context.Resources.GetText(Resource.String.AllProducts)));
            if (catId != null)
            {
                var info = ProductGroupManager.GetGroupInfo((Guid)catId);
                categories.Add(new DrawerProductGroupItem(catId.ToString(), info.GroupName, DrawerProductGroupItem.ItemTypes.Leaf));
            }
            if (temp != null)
            {
                foreach (var item in temp)
                {
                    var it = new DrawerProductGroupItem(item.UniqueId.ToString(), item.GroupName);
                    categories.Add(it);
                }
            }
            MenuItems = categories;
            _drawerListView.Adapter = new DrawerMenuItems(MenuItems, _context);
            _drawerListView.InvalidateViews();
        }

        public void RefreshMenuItems()
        {
            var mainItems = new List<DrawerItemType>();
            if (Customer != null)
            {
                var avatarMenuEntry = new DrawerMainItem();
                avatarMenuEntry.ItemId = DrawerMainItem.DrawerMainItems.Profile;
                if (Customer != null)
                {
                    avatarMenuEntry.Name = Customer.FirstName + " " + Customer.LastName;
                    if (Customer.UniqueId != null)
                    {
                        avatarMenuEntry.ImageUrl = CustomerManager.GetImageAddress(Customer.UniqueId);
                    }
                }
                else
                    avatarMenuEntry.Name = "";
                avatarMenuEntry.ImageResId = Resource.Drawable.ic_person_gray_24dp;
                mainItems.Add(avatarMenuEntry);

            }
            else
            {
                var loginMenuEntry = new DrawerMainItem();
                loginMenuEntry.ItemId = DrawerMainItem.DrawerMainItems.Login;
                loginMenuEntry.Name = "ورود";
                loginMenuEntry.ImageResId = Resource.Drawable.ic_log_in_green_24dp;
                mainItems.Add(loginMenuEntry);
            }
            var firstPageMenuEntry = new DrawerMainItem();
            firstPageMenuEntry.ItemId = DrawerMainItem.DrawerMainItems.FirstPage;
            firstPageMenuEntry.Name = _context.Resources.GetText(Resource.String.FirstPage);
            mainItems.Add(firstPageMenuEntry);

            var categoriesMenuEntry = new DrawerMainItem();
            categoriesMenuEntry.ItemId = DrawerMainItem.DrawerMainItems.ProductCategries;
            categoriesMenuEntry.Name = _context.Resources.GetText(Resource.String.Products);
            categoriesMenuEntry.ImageResId = Resource.Drawable.ic_list_orange_24dp;
            mainItems.Add(categoriesMenuEntry);

            var favoritsMenuEntry = new DrawerMainItem();
            favoritsMenuEntry.ItemId = DrawerMainItem.DrawerMainItems.Favorits;
            favoritsMenuEntry.Name = _context.Resources.GetText(Resource.String.MyList);
            favoritsMenuEntry.ImageResId = Resource.Drawable.ic_mylist_orange_24dp;
            mainItems.Add(favoritsMenuEntry);

            var storesMenuEntry = new DrawerMainItem();
            storesMenuEntry.ItemId = DrawerMainItem.DrawerMainItems.StoresList;
            if (DefaultStore != null)
                storesMenuEntry.Name = _context.Resources.GetText(Resource.String.MyStore) + " ( " + DefaultStore.storeName + " ) ";
            else
                storesMenuEntry.Name = _context.Resources.GetText(Resource.String.MyStore);
            mainItems.Add(storesMenuEntry);

            if (AnatoliUser != null)
            {
                var ordersMenuEntry = new DrawerMainItem();
                ordersMenuEntry.ItemId = DrawerMainItem.DrawerMainItems.Orders;
                ordersMenuEntry.Name = _context.Resources.GetText(Resource.String.LastOrders);
                mainItems.Add(ordersMenuEntry);
            }

            var aboutMenuEntry = new DrawerMainItem();
            aboutMenuEntry.ItemId = DrawerMainItem.DrawerMainItems.About;
            aboutMenuEntry.Name = "درباره ایگ";
            mainItems.Add(aboutMenuEntry);

            MenuItems = mainItems;
            _drawerListView.Adapter = new DrawerMenuItems(MenuItems, _context);
            _drawerListView.InvalidateViews();
        }

        public class StackItem
        {
            public string FragmentName;
            public Type FragmentType;
            public AnatoliFragment Fragment;
            public StackItem(string FragmentName, AnatoliFragment fragment)
            {
                this.FragmentName = FragmentName;
                this.FragmentType = fragment.GetType();
                Fragment = fragment;
            }
        }

        string _locationProvider = "";
        bool _canclelLocation = false;
        bool _locationDialog = true;
        public void StartLocationUpdates()
        {
            try
            {
                Criteria criteriaForLocationService = new Criteria
                {
                    Accuracy = Accuracy.Fine,
                    PowerRequirement = Power.Medium
                };
                _locationProvider = LocationManager.GetBestProvider(criteriaForLocationService, true);
                if (!string.IsNullOrEmpty(_locationProvider) && !_canclelLocation)
                {
                    LocationManager.RequestLocationUpdates(_locationProvider, 10000, 100, (ILocationListener)_context);
                    if (_locationProvider != LocationManager.GpsProvider)
                    {
                        if (_locationDialog)
                        {
                            AlertDialog.Builder alert = new AlertDialog.Builder(_context);
                            alert.SetMessage("برای فاصله یابی دقیق از فروشگاه ها gps دستگاه خود را روشن نمایید");
                            alert.SetPositiveButton("روشن کردن gps", (s, e) =>
                            {
                                Intent callGPSSettingIntent = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
                                _context.StartActivity(callGPSSettingIntent);
                            });
                            alert.SetNegativeButton("بی خیال", (s, e) => { _canclelLocation = true; });
                            alert.Show();
                            _locationDialog = false;
                        }

                    }
                }
                else
                {

                }

            }
            catch (Exception ex)
            {
                ex.SendTrace();
            }
        }

        public void StopLocationUpdates()
        {
            LocationManager.RemoveUpdates((ILocationListener)_context);
        }

        public void SetLocation(Location location)
        {
            OnLocationChanged(location);
        }

        void OnLocationChanged(Location location)
        {
            if (LocationChanged != null)
            {
                LocationChanged.Invoke(location);
            }
        }
        public event LocationChangedEventHandler LocationChanged;
        public delegate void LocationChangedEventHandler(Location location);

        internal bool Logout()
        {
            var result = AnatoliUserManager.Logout();
            if (result)
            {
                AnatoliUser = null;
                RefreshMenuItems();
            }
            return result;
        }

        internal async Task LoginAsync(AnatoliUserModel userModel)
        {
            AnatoliUser = userModel;
            try
            {
                Customer = await CustomerManager.ReadCustomerAsync();
                RefreshMenuItems();
            }
            catch (Exception e)
            {
                e.SendTrace();
            }
            AnatoliApp.GetInstance().RefreshMenuItems();
        }
    }
}