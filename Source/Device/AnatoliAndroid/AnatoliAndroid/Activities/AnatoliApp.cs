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
using Anatoli.App;
using AnatoliAndroid.Components;
using Anatoli.Framework.AnatoliBase;
using Anatoli.App.Model;
using System.Threading.Tasks;
using Anatoli.App.Model.Product;

namespace AnatoliAndroid.Activities
{
    class AnatoliApp
    {
        private static AnatoliApp instance;
        private Activity _activity;
        public Android.Locations.LocationManager LocationManager;
        public AnatoliUserModel AnatoliUser;
        public List<DrawerItemType> AnatoliMenuItems;
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
        double _price;
        public string DefaultStore;
        public TextView ShoppingCardItemCount { get { return _shoppingCardTextView; } }
        public void SetTotalPrice(double price)
        {
            _price = price;
            if (!_shoppingCardTextView.Text.Equals("0"))
                _shoppingPriceTextView.Visibility = ViewStates.Visible;
            else
                _shoppingPriceTextView.Visibility = ViewStates.Invisible;
            _shoppingPriceTextView.Text = _price.ToCurrency() + " تومان";
        }
        public double GetTotalPrice()
        {
            return _price;
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

        public ProductsListFragment ProductsListF;
        StoresListFragment _storesListF;
        FavoritsListFragment _favoritsFragment;
        ShoppingCardFragment _shoppingCardFragment;
        ProfileFragment _profileFragment;
        MessagesListFragment _messagesFragment;
        OrdersListFragment _ordersFragment;

        ProductManager _pm;
        /// <summary>
        /// Shortcut for AnatoliApp.GetInstance().Activity.Resources
        /// </summary>
        /// <returns></returns>
        public static Android.Content.Res.Resources GetResources()
        {
            return AnatoliApp.GetInstance().Activity.Resources;
        }
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

            InputMethodManager inputMethodManager = _activity.GetSystemService(Context.InputMethodService) as InputMethodManager;
            inputMethodManager.ShowSoftInput(pView, ShowFlags.Forced);
            inputMethodManager.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.ImplicitOnly);
        }
        public void HideKeyboard(View pView)
        {
            InputMethodManager inputMethodManager = _activity.GetSystemService(Context.InputMethodService) as InputMethodManager;
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
            _searchEditText.EditorAction += async (s, e) =>
            {
                if (e.ActionId == ImeAction.Done)
                {
                    await Search(_searchEditText.Text);
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
            _searchButtonImageButton.Click += async (s, e) =>
            {
                DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
                await Search(_searchEditText.Text);
            };


            _autoCompleteOptions = new String[] { "نوشیدنی", "لبنیات", "پروتئینی", "خواربار", "روغن", "پنیر", "شیر", "ماست", "کره", "دوغ", "گوشت" };
            _autoCompleteAdapter = new ArrayAdapter(_activity, Resource.Layout.AutoCompleteDropDownLayout, _autoCompleteOptions);

            _searchEditText.Adapter = _autoCompleteAdapter;
            _searchEditText.TextChanged += async (s, e) =>
            {
                if (e.AfterCount >= 3)
                {
                    if (AnatoliApp.GetInstance().GetCurrentFragmentType() == typeof(AnatoliAndroid.Fragments.ProductsListFragment))
                    {
                        var options = (await Anatoli.App.Manager.ProductManager.GetSuggests(_searchEditText.Text, 20));
                        if (options != null)
                        {
                            _autoCompleteOptions = options.ToArray();
                            _autoCompleteAdapter = new ArrayAdapter(_activity, Resource.Layout.AutoCompleteDropDownLayout, _autoCompleteOptions);
                            _searchEditText.Adapter = _autoCompleteAdapter;
                            _searchEditText.Invalidate();
                        }

                    }
                }
            };
            _searchEditText.ItemClick += async (s, e) => { await Search(_searchEditText.Text); };
            _shoppingCardImageButton = ToolBar.FindViewById<ImageButton>(Resource.Id.shoppingCardImageButton);
            var shoppingbarRelativeLayout = ToolBar.FindViewById<RelativeLayout>(Resource.Id.shoppingbarRelativeLayout);
            shoppingbarRelativeLayout.Click += shoppingbarRelativeLayout_Click;
            _shoppingCardImageButton.Click += shoppingbarRelativeLayout_Click;
            _menuIconImageButton = ToolBar.FindViewById<ImageButton>(Resource.Id.menuImageButton);
            _menuIconImageButton.Click += (s, e) => { OnMenuClick(); };
        }

        void shoppingbarRelativeLayout_Click(object sender, EventArgs e)
        {
            if (AnatoliApp.GetInstance().AnatoliUser == null)
            {
                Toast.MakeText(AnatoliApp.GetInstance().Activity, Resource.String.PleaseLogin, ToastLength.Short).Show();
                LoginFragment login = new LoginFragment();
                var transaction = AnatoliApp.GetInstance().Activity.FragmentManager.BeginTransaction();
                login.LoginSuceeded += () => { SetFragment<AnatoliAndroid.Fragments.ShoppingCardFragment>(_shoppingCardFragment, "shoppingCard_fragment"); };
                login.Show(transaction, "login_fragment");
                return;
            }
            SetFragment<AnatoliAndroid.Fragments.ShoppingCardFragment>(_shoppingCardFragment, "shoppingCard_fragment");
            DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
        }

        async System.Threading.Tasks.Task Search(string value)
        {
            value = value.Trim();
            if (String.IsNullOrEmpty(value))
                return;
            if (AnatoliApp.GetInstance().GetCurrentFragmentType() == typeof(ProductsListFragment))
            {
                if (ProductsListF == null)
                {
                    ProductsListF = _currentFragment as ProductsListFragment;
                }

                await ProductsListF.Search(ProductManager.Search(value), value);

            }
            if (AnatoliApp.GetInstance().GetCurrentFragmentType() == typeof(FirstFragment))
            {
                ProductsListF = SetFragment(ProductsListF, "products_fragment");
                ProductsListF.SetCatId(null);
                await ProductsListF.Search(ProductManager.Search(value), value);
            }
            if (GetInstance().GetCurrentFragmentType() == typeof(StoresListFragment))
            {
                if (_storesListF == null)
                {
                    _storesListF = _currentFragment as StoresListFragment;
                }
                await _storesListF.Search(StoreManager.Search(value), value);
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
            if (_searchBar)
            {

            }
            else
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
        Fragment _currentFragment;
        public Activity Activity { get { return _activity; } }
        public static AnatoliApp GetInstance()
        {
            if (instance == null)
                throw new NullReferenceException();
            return instance;
        }
        public static void Initialize(Activity activity, AnatoliUserModel user, ListView drawerListView, Toolbar toolbar)
        {
            instance = new AnatoliApp(activity, user, drawerListView, toolbar);
            _list = new LinkedList<StackItem>();
        }
        private AnatoliApp()
        {

        }

        private AnatoliApp(Activity activity, AnatoliUserModel user, ListView drawerListView, Toolbar toolbar)
        {
            _activity = activity;
            AnatoliUser = user;
            _drawerListView = drawerListView;
            _drawerListView.ItemClick += _drawerListView_ItemClick;
            ToolBar = toolbar;
            CreateToolbar();
            _pm = new ProductManager();
        }
        async void _drawerListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var selectedItem = AnatoliApp.GetInstance().AnatoliMenuItems[e.Position];
            GetInstance().DrawerListView.SetItemChecked(e.Position, true);
            if (selectedItem.GetType() == typeof(DrawerMainItem))
            {
                switch (selectedItem.ItemId)
                {
                    case DrawerMainItem.DrawerMainItems.ProductCategries:
                        bool go = true;
                        if ((await SyncManager.GetLastUpdateDateAsync("products_price")) == DateTime.MinValue)
                        {
                            go = await AnatoliApp.GetInstance().SyncDatabase();
                        }
                        if (go)
                        {
                            if (ProductsListF != null)
                            {
                                ProductsListF.SetCatId(null);
                            }
                            var temp = await CategoryManager.GetFirstLevelAsync();
                            var categories = new List<DrawerItemType>();
                            categories.Add(new DrawerMainItem(DrawerMainItem.DrawerMainItems.MainMenu, AnatoliApp.GetResources().GetText(Resource.String.MainMenu)));
                            categories.Add(new DrawerMainItem(DrawerMainItem.DrawerMainItems.AllProducts, AnatoliApp.GetResources().GetText(Resource.String.AllProducts)));
                            if (temp != null)
                            {
                                foreach (var item in temp)
                                {
                                    var it = new DrawerPCItem(item.cat_id.ToString(), item.cat_name);
                                    categories.Add(it);
                                }
                                AnatoliApp.GetInstance().RefreshMenuItems(categories);
                                ProductsListF = AnatoliApp.GetInstance().SetFragment<ProductsListFragment>(ProductsListF, "products_fragment");
                            }
                        }

                        break;
                    case DrawerMainItem.DrawerMainItems.ShoppingCard:
                        if (AnatoliApp.GetInstance().AnatoliUser == null)
                        {
                            Toast.MakeText(AnatoliApp.GetInstance().Activity, Resource.String.PleaseLogin, ToastLength.Short).Show();
                            LoginFragment login = new LoginFragment();
                            login.LoginSuceeded += () => { _shoppingCardFragment = AnatoliApp.GetInstance().SetFragment<ShoppingCardFragment>(_shoppingCardFragment, "shoppingCard_fragment"); };
                            var tt = AnatoliApp.GetInstance().Activity.FragmentManager.BeginTransaction();
                            login.Show(tt, "login_fragment");
                            break;
                        }
                        DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
                        _shoppingCardFragment = AnatoliApp.GetInstance().SetFragment<ShoppingCardFragment>(_shoppingCardFragment, "shoppingCard_fragment");
                        break;
                    case DrawerMainItem.DrawerMainItems.StoresList:
                        DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
                        _storesListF = AnatoliApp.GetInstance().SetFragment<StoresListFragment>(_storesListF, "stores_fragment");
                        break;
                    case DrawerMainItem.DrawerMainItems.FirstPage:
                        DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
                        AnatoliApp.GetInstance().SetFragment<FirstFragment>(new FirstFragment(), "first_fragment");
                        break;
                    case DrawerMainItem.DrawerMainItems.Login:
                        DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
                        var transaction = Activity.FragmentManager.BeginTransaction();
                        var loginFragment = new LoginFragment();
                        loginFragment.LoginSuceeded += () => { ProductsListF = AnatoliApp.GetInstance().SetFragment<ProductsListFragment>(ProductsListF, "products_fragment"); };
                        loginFragment.Show(transaction, "shipping_dialog");
                        break;
                    case DrawerMainItem.DrawerMainItems.MainMenu:
                        AnatoliApp.GetInstance().RefreshMenuItems();
                        break;
                    case DrawerMainItem.DrawerMainItems.Favorits:
                        DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
                        _favoritsFragment = new FavoritsListFragment();
                        AnatoliApp.GetInstance().SetFragment<FavoritsListFragment>(_favoritsFragment, "favorits_fragment");
                        break;
                    case DrawerMainItem.DrawerMainItems.Avatar:
                        DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
                        _profileFragment = new ProfileFragment();
                        var tr = _activity.FragmentManager.BeginTransaction();
                        _profileFragment.Show(tr, "profile_fragment");
                        break;
                    case DrawerMainItem.DrawerMainItems.Messages:
                        DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
                        _messagesFragment = new MessagesListFragment();
                        AnatoliApp.GetInstance().SetFragment<MessagesListFragment>(_messagesFragment, "messages_fragment");
                        break;
                    case DrawerMainItem.DrawerMainItems.Orders:
                        DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
                        _ordersFragment = new OrdersListFragment();
                        AnatoliApp.GetInstance().SetFragment<OrdersListFragment>(_ordersFragment, "orders_fragment");
                        break;
                    case DrawerMainItem.DrawerMainItems.Update:
                        DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
                        ProgressDialog pDialog = new ProgressDialog(_activity);
                        var g = await SyncDatabase();
                        SetFragment<FirstFragment>(null, "first_fragment)");
                        break;
                    case DrawerMainItem.DrawerMainItems.Logout:
                        DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
                        bool result = await AnatoliUserManager.LogoutAsync();
                        if (result)
                        {
                            AnatoliApp.GetInstance().AnatoliUser = null;
                            AnatoliApp.GetInstance().RefreshMenuItems();
                            AnatoliApp.GetInstance().SetFragment<ProductsListFragment>(ProductsListF, "products_fragment");
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                if (ProductsListF != null)
                {
                }
                if ((selectedItem as DrawerPCItem).ItemType == DrawerPCItem.ItemTypes.Leaf)
                {
                    ProductsListF.SetCatId(selectedItem.ItemId);
                    SetFragment<ProductsListFragment>(ProductsListF, "products_fragment");
                    AnatoliApp.GetInstance()._toolBarTextView.Text = selectedItem.Name;
                    DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
                    return;
                }
                var temp = await CategoryManager.GetCategoriesAsync(selectedItem.ItemId);
                if (temp != null)
                {
                    ProductsListF.SetCatId(selectedItem.ItemId);
                    var categories = new List<DrawerItemType>();
                    categories.Add(new DrawerMainItem(DrawerMainItem.DrawerMainItems.MainMenu, AnatoliApp.GetResources().GetText(Resource.String.MainMenu)));
                    var parent = await CategoryManager.GetParentCategory(selectedItem.ItemId);
                    var current = await CategoryManager.GetCategoryInfo(selectedItem.ItemId);
                    if (current != null)
                    {
                        if (parent != null)
                        {
                            categories.Add(new DrawerPCItem(parent.cat_id.ToString(), parent.cat_name, DrawerPCItem.ItemTypes.Parent));
                            categories.Add(new DrawerPCItem(current.cat_id.ToString(), current.cat_name, DrawerPCItem.ItemTypes.Leaf));
                        }
                        else
                        {
                            categories.Add(new DrawerMainItem(DrawerMainItem.DrawerMainItems.ProductCategries, AnatoliApp.GetResources().GetText(Resource.String.AllProducts)));
                            categories.Add(new DrawerPCItem(current.cat_id.ToString(), current.cat_name, DrawerPCItem.ItemTypes.Leaf));
                        }
                        foreach (var item in temp)
                        {
                            var it = new DrawerPCItem(item.cat_id.ToString(), item.cat_name);
                            categories.Add(it);
                        }
                        AnatoliApp.GetInstance().RefreshMenuItems(categories);
                        AnatoliApp.GetInstance()._toolBarTextView.Text = selectedItem.Name;
                        if (temp.Count == 0)
                        {
                            AnatoliApp.GetInstance()._toolBarTextView.Text = selectedItem.Name;
                            DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
                        }
                    }
                }
                else
                {
                    AnatoliApp.GetInstance()._toolBarTextView.Text = selectedItem.Name;
                    DrawerLayout.CloseDrawer(AnatoliApp.GetInstance().DrawerListView);
                }

            }
        }
        internal async Task<CustomerViewModel> RefreshCutomerProfile(bool cancelable = false)
        {
            if (AnatoliClient.GetInstance().WebClient.IsOnline() && AnatoliApp.GetInstance().AnatoliUser != null)
            {
                AlertDialog.Builder errDialog = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                Android.App.ProgressDialog pDialog = new Android.App.ProgressDialog(_activity);
                pDialog.SetTitle(AnatoliApp.GetResources().GetText(Resource.String.Updating));
                pDialog.SetMessage(AnatoliApp.GetResources().GetText(Resource.String.PleaseWait));
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
                    pDialog.Dismiss();
                    if (c.IsValid)
                    {
                        await CustomerManager.SaveCustomerAsync(c);
                    }
                    return c;
                }
                catch (Exception)
                {
                    errDialog.SetMessage(Resource.String.ErrorOccured);
                    errDialog.SetPositiveButton(Resource.String.Ok, (s2, e2) => { });
                    errDialog.Show();
                }
            }
            return null;
        }
        async Task CancelSync()
        {
            if ((await SyncManager.GetLastUpdateDateAsync("products_price")) == DateTime.MinValue)
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                alert.SetMessage("هیچ اطلاعاتی در درسترس نیست. لطفا دوباره تلاش کنید");
                alert.SetNegativeButton("بی خیال و خروج", (s3, e3) => { _activity.Finish(); });
                alert.SetPositiveButton("دوباره تلاش کن", async (s3, e3) => { await SyncDatabase(); });
                alert.Show();
            }
        }
        internal async Task<bool> SyncDatabase()
        {
            Android.App.ProgressDialog pDialog = new Android.App.ProgressDialog(_activity);
            pDialog.SetTitle(AnatoliApp.GetResources().GetText(Resource.String.Updating) + " 1 از 6");
            pDialog.SetMessage(" بروز رسانی لیست شهر ها");
            System.Threading.CancellationTokenSource tokenSource = new System.Threading.CancellationTokenSource();
            pDialog.CancelEvent += async (s, e) => { tokenSource.Cancel(); await CancelSync(); };
            pDialog.SetButton("بی خیال", async (s, e) =>
            {
                tokenSource.Cancel();
                await CancelSync();
            });
            pDialog.Show();
            try
            {
                await BaseTypeManager.SyncDataBase(tokenSource);
                await CityRegionManager.SyncDataBase(tokenSource);
                pDialog.SetTitle(AnatoliApp.GetResources().GetText(Resource.String.Updating) + " 2 از 6");
                pDialog.SetMessage("بروز رسانی لیست فروشگاه ها");
                await StoreManager.SyncDataBase(tokenSource);
                pDialog.SetTitle(AnatoliApp.GetResources().GetText(Resource.String.Updating) + " 3 از 6");
                pDialog.SetMessage("بروز رسانی گروه کالاها");
                await CategoryManager.SyncDataBase(tokenSource);
                pDialog.SetTitle(AnatoliApp.GetResources().GetText(Resource.String.Updating) + " 4 از 6");
                pDialog.SetMessage("بروز رسانی لیست کالاها");
                await ProductManager.SyncProducts(tokenSource);
                pDialog.SetTitle(AnatoliApp.GetResources().GetText(Resource.String.Updating) + " 5 از 6");
                pDialog.SetMessage("بروز رسانی تصاویر");
                await ItemImageManager.SyncDataBase(tokenSource);
                pDialog.SetTitle(AnatoliApp.GetResources().GetText(Resource.String.Updating) + " 6 از 6");
                pDialog.SetMessage("بروز رسانی قیمت ها");
                await ProductManager.SyncPrices(tokenSource);
                await SyncManager.SaveDBVersionAsync();
                pDialog.Dismiss();
                return true;
            }
            catch (Exception ex)
            {
                pDialog.Dismiss();
                AlertDialog.Builder alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                if (ex.GetType() == typeof(Anatoli.Framework.Helper.SyncPolicyHelper.SyncPolicyException))
                {
                    alert.SetMessage(Resource.String.PleaseConnectToInternet);
                    alert.SetPositiveButton(Resource.String.Ok, (s2, e2) =>
                    {
                        Intent intent = new Intent(Android.Provider.Settings.ActionSettings);
                        AnatoliApp.GetInstance().Activity.StartActivity(intent);
                    });
                }
                else
                    alert.SetMessage(Resource.String.ErrorOccured);
                alert.SetNegativeButton(Resource.String.Cancel, async (s2, e2) =>
                {
                    await CancelSync();
                });
                alert.SetTitle(Resource.String.Error);
                alert.Show();
                return false;
            }
        }
        internal FragmentType SetFragment<FragmentType>(FragmentType fragment, string tag, Tuple<string, string> parameter) where FragmentType : Android.App.Fragment, new()
        {
            if (fragment == null)
                fragment = new FragmentType();
            Bundle bundle = new Bundle();
            bundle.PutString(parameter.Item1, parameter.Item2);
            fragment.Arguments = bundle;
            return SetFragment(fragment, tag);
        }
        public FragmentType SetFragment<FragmentType>(FragmentType fragment, string tag) where FragmentType : Android.App.Fragment, new()
        {
            var transaction = _activity.FragmentManager.BeginTransaction();
            if (fragment == null)
            {
                fragment = new FragmentType();
            }
            transaction.Replace(Resource.Id.content_frame, fragment, tag);
            transaction.Commit();
            _toolBarTextView.Text = fragment.GetTitle();
            foreach (var item in _list)
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
            _list.AddLast(new StackItem(tag, fragment.GetType()));
            _currentFragment = fragment;
            if (_currentFragment.GetType() != typeof(ProductsListFragment))
            {
                RefreshMenuItems();
            }
            _backToExit = 0;
            return fragment;
        }
        public bool BackFragment()
        {
            var transaction = _activity.FragmentManager.BeginTransaction();
            try
            {
                if (_list.Last<StackItem>().FragmentType == typeof(FirstFragment))
                {
                    _backToExit++;
                }
                if (_list.Last<StackItem>().FragmentType != typeof(FirstFragment) || _backToExit >= 2)
                {
                    _list.RemoveLast();
                    var stackItem = _list.Last<StackItem>();
                    if (stackItem.FragmentType == typeof(AnatoliAndroid.Fragments.LoginFragment) && AnatoliUser != null)
                    {
                        _list.RemoveLast();
                        stackItem = _list.Last<StackItem>();
                    }
                    if (stackItem.FragmentType == typeof(AnatoliAndroid.Fragments.ProfileFragment) && AnatoliUser == null)
                    {
                        _list.RemoveLast();
                        stackItem = _list.Last<StackItem>();
                    }
                    var fragment = Activator.CreateInstance(stackItem.FragmentType);
                    transaction.Replace(Resource.Id.content_frame, fragment as Fragment, stackItem.FragmentName);
                    transaction.Commit();
                    _toolBarTextView.Text = (fragment as Fragment).GetTitle();
                    if (fragment.GetType() != typeof(ProductsListFragment))
                    {
                        RefreshMenuItems();
                    }
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public void RefreshMenuItems(List<DrawerItemType> items = null)
        {
            if (items != null)
            {
                AnatoliMenuItems = items;
                _drawerListView.Adapter = new DrawerMenuItems(AnatoliMenuItems, _activity);
                _drawerListView.InvalidateViews();
                return;
            }

            var mainItems = new List<DrawerItemType>();
            if (AnatoliUser != null)
            {

                var avatarMenuEntry = new DrawerMainItem();
                avatarMenuEntry.ItemId = DrawerMainItem.DrawerMainItems.Avatar;
                avatarMenuEntry.Name = AnatoliUser.FullName;
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
            firstPageMenuEntry.Name = AnatoliApp.GetResources().GetText(Resource.String.FirstPage);
            mainItems.Add(firstPageMenuEntry);


            var categoriesMenuEntry = new DrawerMainItem();
            categoriesMenuEntry.ItemId = DrawerMainItem.DrawerMainItems.ProductCategries;
            categoriesMenuEntry.Name = AnatoliApp.GetResources().GetText(Resource.String.Products);
            categoriesMenuEntry.ImageResId = Resource.Drawable.ic_list_orange_24dp;
            mainItems.Add(categoriesMenuEntry);


            var favoritsMenuEntry = new DrawerMainItem();
            favoritsMenuEntry.ItemId = DrawerMainItem.DrawerMainItems.Favorits;
            favoritsMenuEntry.Name = AnatoliApp.GetResources().GetText(Resource.String.MyList);
            favoritsMenuEntry.ImageResId = Resource.Drawable.ic_mylist_orange_24dp;
            mainItems.Add(favoritsMenuEntry);

            var storesMenuEntry = new DrawerMainItem();
            storesMenuEntry.ItemId = DrawerMainItem.DrawerMainItems.StoresList;
            if (DefaultStore != null)
                storesMenuEntry.Name = AnatoliApp.GetResources().GetText(Resource.String.MyStore) + " ( " + DefaultStore + " ) ";
            else
                storesMenuEntry.Name = AnatoliApp.GetResources().GetText(Resource.String.MyStore);
            mainItems.Add(storesMenuEntry);

            if (AnatoliUser != null)
            {
                var msgMenuEntry = new DrawerMainItem();
                msgMenuEntry.ItemId = DrawerMainItem.DrawerMainItems.Messages;
                msgMenuEntry.Name = AnatoliApp.GetResources().GetText(Resource.String.Messages);
                mainItems.Add(msgMenuEntry);

                var ordersMenuEntry = new DrawerMainItem();
                ordersMenuEntry.ItemId = DrawerMainItem.DrawerMainItems.Orders;
                ordersMenuEntry.Name = AnatoliApp.GetResources().GetText(Resource.String.LastOrders);
                mainItems.Add(ordersMenuEntry);
            }

            //var shoppingCardMenuEntry = new DrawerMainItem();
            //shoppingCardMenuEntry.ItemId = DrawerMainItem.DrawerMainItems.ShoppingCard;
            //shoppingCardMenuEntry.Name = "سبد خرید";
            //shoppingCardMenuEntry.ImageResId = Resource.Drawable.ShoppingCardRed;
            //mainItems.Add(shoppingCardMenuEntry);



            //if (AnatoliUser != null)
            //{
            //    var profileMenuEntry = new DrawerMainItem();
            //    profileMenuEntry.ItemId = DrawerMainItem.DrawerMainItems.Profile;
            //    profileMenuEntry.Name = "مشخصات من";
            //    profileMenuEntry.ImageResId = Resource.Drawable.Profile;
            //    mainItems.Add(profileMenuEntry);

            //}

            //var helpMenuEntry = new DrawerMainItem();
            //helpMenuEntry.ItemId = DrawerMainItem.DrawerMainItems.Help;
            //helpMenuEntry.Name = "راهنما";
            //mainItems.Add(helpMenuEntry);

            //if (AnatoliUser != null)
            //{
            //    var logoutMenuEntry = new DrawerMainItem();
            //    logoutMenuEntry.ItemId = DrawerMainItem.DrawerMainItems.Logout;
            //    logoutMenuEntry.Name = "خروج";
            //    logoutMenuEntry.ImageResId = Resource.Drawable.Exit;
            //    mainItems.Add(logoutMenuEntry);
            //}

            var updateMenuEntry = new DrawerMainItem();
            updateMenuEntry.ItemId = DrawerMainItem.DrawerMainItems.Update;
            updateMenuEntry.Name = AnatoliApp.GetResources().GetText(Resource.String.Update);
            mainItems.Add(updateMenuEntry);


            AnatoliMenuItems = mainItems;
            _drawerListView.Adapter = new DrawerMenuItems(AnatoliMenuItems, _activity);
            _drawerListView.InvalidateViews();
        }

        public class StackItem
        {
            public string FragmentName;
            public Type FragmentType;
            public StackItem(string FragmentName, Type FragmentType)
            {
                this.FragmentName = FragmentName;
                this.FragmentType = FragmentType;
            }
        }

        string _locationProvider = "";
        bool _canclelLocation = false;

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
                if (!String.IsNullOrEmpty(_locationProvider) && !_canclelLocation)
                {
                    LocationManager.RequestLocationUpdates(_locationProvider, 10000, 100, (ILocationListener)_activity);
                    if (_locationProvider != LocationManager.GpsProvider)
                    {
                        AlertDialog.Builder alert = new AlertDialog.Builder(_activity);
                        alert.SetMessage("برای فاصله یابی دقیق از فروشگاه ها gps دستگاه خود را روشن نمایید");
                        alert.SetPositiveButton("روشن کردن gps", (s, e) =>
                        {
                            Intent callGPSSettingIntent = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
                            _activity.StartActivity(callGPSSettingIntent);
                        });
                        alert.SetNegativeButton("بی خیال", (s, e) => { _canclelLocation = true; });
                        alert.Show();
                    }
                }
                else
                {

                }

            }
            catch (Exception)
            {

            }
        }

        public void StopLocationUpdates()
        {
            LocationManager.RemoveUpdates((ILocationListener)_activity);
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
    }
}