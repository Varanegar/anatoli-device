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
using Anatoli.App.Model.Product;
using Anatoli.App.Manager;
using AnatoliAndroid.ListAdapters;
using Anatoli.App.Model.AnatoliUser;
using Anatoli.App.Model;
using Anatoli.Framework.AnatoliBase;
using AnatoliAndroid.Activities;
using System.Threading.Tasks;
using Android.Animation;
using Android.Views.InputMethods;
using Anatoli.App.Model.Store;
using AnatoliAndroid.Components;
using Anatoli.Framework;

namespace AnatoliAndroid.Fragments
{
    class ShoppingCardFragment : AnatoliFragment
    {
        ListView _itemsListView;
        TextView _itemCountTextView;
        ProductsListAdapter _listAdapter;
        RelativeLayout _countRelativeLayout;
        RelativeLayout _cardItemsRelativeLayout;
        TextView _deliveryAddress;
        TextView _factorePriceTextView;
        TextView _storeTelTextView;
        TextView _countTextView;
        Spinner _delivaryDate;
        AnatoliListBox<DeliveryTimeListAdapter, DeliveryTimeManager, DeliveryTimeModel> _deliveryTimeListBox;
        ImageView _slideupmageView;
        ImageView _slidedownImageView;
        Button _checkoutButton;
        ImageButton _callImageButton;
        ImageButton _editAddressImageButton;
        List<DeliveryTimeModel> _timeOptions;
        List<DeliveryTypeModel> _typeOptions;
        AnatoliListBox<DeliveryTypeListAdapter, DeliveryTypeManager, DeliveryTypeModel> _deliveryTypeListBox;
        bool _tomorrow = false;
        CustomerViewModel _customerViewModel;
        Cheesebaron.SlidingUpPanel.SlidingUpPanelLayout _slidingLayout;

        public override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var view = inflater.Inflate(Resource.Layout.ShoppingCardLayout, container, false);
            _itemsListView = view.FindViewById<ListView>(Resource.Id.shoppingCardListView);
            _itemCountTextView = view.FindViewById<TextView>(Resource.Id.itemCountTextView);
            _countRelativeLayout = view.FindViewById<RelativeLayout>(Resource.Id.countRelativeLayout);
            _cardItemsRelativeLayout = view.FindViewById<RelativeLayout>(Resource.Id.cardItemsRelativeLayout);

            _checkoutButton = view.FindViewById<Button>(Resource.Id.checkoutButton);
            _checkoutButton.UpdateWidth();

            _slideupmageView = view.FindViewById<ImageView>(Resource.Id.slideupImageView);
            _slidedownImageView = view.FindViewById<ImageView>(Resource.Id.slidedownImageView);
            _slidedownImageView.Visibility = ViewStates.Gone;

            _slidingLayout = view.FindViewById<Cheesebaron.SlidingUpPanel.SlidingUpPanelLayout>(Resource.Id.sliding_layout);
            _slidingLayout.DragView = _countRelativeLayout;
            _slidingLayout.PanelExpanded += (s, e) =>
            {
                _cardItemsRelativeLayout.Visibility = ViewStates.Gone;
                _slidedownImageView.Visibility = ViewStates.Visible;
                _slideupmageView.Visibility = ViewStates.Gone;
            };
            _slidingLayout.PanelCollapsed += (s, e) =>
            {
                _cardItemsRelativeLayout.Visibility = ViewStates.Visible;
                _slidedownImageView.Visibility = ViewStates.Gone;
                _slideupmageView.Visibility = ViewStates.Visible;
            };
            _countTextView = view.FindViewById<TextView>(Resource.Id.itemCountTextView);
            _callImageButton = view.FindViewById<ImageButton>(Resource.Id.callImageButton);
            _storeTelTextView = view.FindViewById<TextView>(Resource.Id.storeTelTextView);
            _factorePriceTextView = view.FindViewById<TextView>(Resource.Id.factorPriceTextView);
            _deliveryAddress = view.FindViewById<TextView>(Resource.Id.addressTextView);
            _editAddressImageButton = view.FindViewById<ImageButton>(Resource.Id.editAddressImageButton);
            _delivaryDate = view.FindViewById<Spinner>(Resource.Id.dateSpinner);
            _deliveryTimeListBox = view.FindViewById<AnatoliListBox<DeliveryTimeListAdapter, DeliveryTimeManager, DeliveryTimeModel>>(Resource.Id.timeSpinner);
            _deliveryTypeListBox = view.FindViewById<AnatoliListBox<DeliveryTypeListAdapter, DeliveryTypeManager, DeliveryTypeModel>>(Resource.Id.typeSpinner);


            _checkoutButton.Click += async (s, e) =>
            {
                if (!AnatoliClient.GetInstance().WebClient.IsOnline())
                {
                    AlertDialog.Builder alertDialog = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                    alertDialog.SetMessage(Resource.String.PleaseConnectToInternet);
                    alertDialog.SetTitle(Resource.String.Error);
                    alertDialog.SetPositiveButton(Resource.String.Ok, delegate { });
                    alertDialog.Show();
                    return;
                }
                var store = StoreManager.GetDefault();
                if (AnatoliApp.GetInstance().AnatoliUser == null)
                {
                    AlertDialog.Builder lAlert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                    lAlert.SetMessage(Resource.String.PleaseLogin);
                    lAlert.SetPositiveButton(Resource.String.Ok, delegate
                    {
                        var transaction = AnatoliApp.GetInstance().Activity.FragmentManager.BeginTransaction();
                        var loginFragment = new LoginFragment();
                        loginFragment.Show(transaction, "shipping_dialog");
                    });
                    lAlert.SetNegativeButton(Resource.String.Cancel, delegate { });
                    lAlert.Show();
                    return;
                }
                if (await UpdateShippingInfo())
                {
                    ProgressDialog pDialog = new ProgressDialog(AnatoliApp.GetInstance().Activity);
                    try
                    {
                        if (_tomorrow)
                        {
                            AlertDialog.Builder lAlert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                            lAlert.SetMessage("امکان ارسال برای امروز وجود ندارد. آیا مایل هستید سفارش شما فردا ارسال شود؟");
                            lAlert.SetPositiveButton(Resource.String.Yes, async delegate
                            {
                                pDialog.SetCancelable(false);
                                pDialog.SetMessage(Resources.GetText(Resource.String.PleaseWait));
                                pDialog.SetTitle("در حال ارسال سفارش");
                                pDialog.Show();
                                // "BE2919AB-5564-447A-BE49-65A81E6AF712"
                                var o = await ShoppingCardManager.CalcPromo(AnatoliApp.GetInstance().Customer, _customerViewModel.UniqueId, store.UniqueId, _deliveryTypeListBox.SelectedItem.UniqueId, _deliveryTimeListBox.SelectedItem);
                                pDialog.Dismiss();
                                if (o.IsValid)
                                {
                                    ProformaFragment proforma = new ProformaFragment(o, _customerViewModel);
                                    var fr = AnatoliApp.GetInstance().Activity.FragmentManager.BeginTransaction();
                                    proforma.ProformaAccepted += async delegate
                                    {
                                        ProgressDialog pDialog2 = new ProgressDialog(AnatoliApp.GetInstance().Activity);
                                        pDialog2.SetCancelable(false);
                                        pDialog2.SetMessage("سفارش شما در فاصله زمانی نیم ساعته به دست شما خواهد رسید");
                                        pDialog2.SetTitle("در حال ارسال سفارش");
                                        pDialog2.Show();
                                        try
                                        {
                                            var result = await ShoppingCardManager.Checkout(_customerViewModel, _customerViewModel.UniqueId, store.UniqueId, _deliveryTypeListBox.SelectedItem.UniqueId, _deliveryTimeListBox.SelectedItem);
                                            pDialog2.Dismiss();
                                            if (result == null)
                                            {
                                                AlertDialog.Builder alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                                                alert.SetMessage("عدم دریافت اطلاعات از سرور");
                                                alert.SetTitle(Resource.String.Error);
                                                alert.SetNegativeButton(Resource.String.Ok, delegate { });
                                                alert.Show();
                                            }
                                            else if (result.IsValid)
                                            {
                                                ShoppingCardManager.Clear();
                                                OrderSavedDialogFragment dialog = new OrderSavedDialogFragment();
                                                var transaction = FragmentManager.BeginTransaction();
                                                dialog.Show(transaction, "order_saved_dialog");
                                                proforma.Dismiss();
                                                var orders = new OrdersListFragment();
                                                orders.SetQuery(PurchaseOrderManager.GetOrderQueryString());
                                                AnatoliApp.GetInstance().PushFragment(orders, "orders_fragment");
                                            }
                                            else
                                            {
                                                AlertDialog.Builder alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                                                alert.SetMessage("ارسال سفارش با مشکل مواجه شد");
                                                alert.SetTitle(Resource.String.Error);
                                                alert.SetNegativeButton(Resource.String.Ok, delegate { });
                                                alert.Show();
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            ex.SendTrace();
                                            AlertDialog.Builder alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                                            alert.SetMessage("ارسال سفارش با مشکل مواجه شد");
                                            alert.SetTitle(Resource.String.Error);
                                            alert.SetNegativeButton(Resource.String.Ok, delegate { });
                                            alert.Show();
                                        }
                                        finally
                                        {
                                            pDialog2.Dismiss();
                                        }
                                    };
                                    proforma.Show(fr, "proforma_fragment");
                                }
                                else
                                {
                                    AlertDialog.Builder alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                                    alert.SetMessage("ارسال سفارش با مشکل مواجه شد");
                                    alert.SetTitle(Resource.String.Error);
                                    alert.SetNegativeButton(Resource.String.Ok, delegate { });
                                    alert.Show();
                                }

                            });
                            lAlert.SetNegativeButton(Resource.String.Cancel, delegate
                            {
                                Toast.MakeText(AnatoliApp.GetInstance().Activity, "سفارش شما کنسل شد", ToastLength.Short).Show();
                                AnatoliApp.GetInstance().PushFragment(new FirstFragment(), "products_fragment");
                            });
                            lAlert.Show();
                        }
                        else
                        {
                            pDialog.SetCancelable(false);
                            pDialog.SetMessage(Resources.GetText(Resource.String.PleaseWait));
                            pDialog.SetTitle("در حال ارسال سفارش");
                            pDialog.Show();
                            var o = await ShoppingCardManager.CalcPromo(AnatoliApp.GetInstance().Customer, _customerViewModel.UniqueId, store.UniqueId, _deliveryTypeListBox.SelectedItem.UniqueId, _deliveryTimeListBox.SelectedItem);
                            pDialog.Dismiss();
                            if (o.IsValid)
                            {
                                ProformaFragment proforma = new ProformaFragment(o, _customerViewModel);
                                var fr = AnatoliApp.GetInstance().Activity.FragmentManager.BeginTransaction();
                                proforma.ProformaAccepted += async delegate
                                {
                                    ProgressDialog pDialog2 = new ProgressDialog(AnatoliApp.GetInstance().Activity);
                                    pDialog2.SetCancelable(false);
                                    pDialog2.SetMessage("سفارش شما در فاصله زمانی نیم ساعته به دست شما خواهد رسید");
                                    pDialog2.SetTitle("در حال ارسال سفارش");
                                    pDialog2.Show();
                                    try
                                    {
                                        var result = await ShoppingCardManager.Checkout(_customerViewModel, _customerViewModel.UniqueId, store.UniqueId, _deliveryTypeListBox.SelectedItem.UniqueId, _deliveryTimeListBox.SelectedItem);
                                        pDialog2.Dismiss();
                                        if (result == null)
                                        {
                                            AlertDialog.Builder alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                                            alert.SetMessage("عدم دریافت اطلاعات از سرور");
                                            alert.SetTitle(Resource.String.Error);
                                            alert.SetNegativeButton(Resource.String.Ok, delegate { });
                                            alert.Show();
                                        }
                                        else if (result.IsValid)
                                        {
                                            ShoppingCardManager.Clear();
                                            OrderSavedDialogFragment dialog = new OrderSavedDialogFragment();
                                            var transaction = FragmentManager.BeginTransaction();
                                            dialog.Show(transaction, "order_saved_dialog");
                                            proforma.Dismiss();
                                            var orders = new OrdersListFragment();
                                            orders.SetQuery(PurchaseOrderManager.GetOrderQueryString());
                                            AnatoliApp.GetInstance().PushFragment(orders, "orders_fragment");
                                        }
                                        else
                                        {
                                            AlertDialog.Builder alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                                            alert.SetMessage("ارسال سفارش با مشکل مواجه شد");
                                            alert.SetTitle(Resource.String.Error);
                                            alert.SetNegativeButton(Resource.String.Ok, delegate { });
                                            alert.Show();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ex.SendTrace();
                                        AlertDialog.Builder alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                                        alert.SetMessage("ارسال سفارش با مشکل مواجه شد");
                                        alert.SetTitle(Resource.String.Error);
                                        alert.SetNegativeButton(Resource.String.Ok, delegate { });
                                        alert.Show();
                                    }
                                    finally
                                    {
                                        pDialog2.Dismiss();
                                    }
                                };
                                proforma.Show(fr, "proforma_fragment");
                                pDialog.Dismiss();
                            }
                            else
                            {
                                AlertDialog.Builder alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                                alert.SetMessage("ارسال سفارش با مشکل مواجه شد");
                                alert.SetTitle(Resource.String.Error);
                                alert.SetNegativeButton(Resource.String.Ok, delegate { });
                                alert.Show();
                            }
                            pDialog.Dismiss();
                        }

                    }
                    catch (Exception ex)
                    {
                        ex.SendTrace();
                        AlertDialog.Builder alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                        alert.SetMessage("ارسال سفارش با مشکل مواجه شد");
                        alert.SetTitle(Resource.String.Error);
                        alert.SetNegativeButton(Resource.String.Ok, delegate { });
                        alert.Show();
                    }
                    finally
                    {
                        pDialog.Dismiss();
                    }

                }


            };

            _deliveryTimeListBox.SelectItem(0);
            _editAddressImageButton.Click += (s, e) =>
            {
                var transaction = FragmentManager.BeginTransaction();
                EditShippingInfoFragment editShippingDialog = new EditShippingInfoFragment();
                editShippingDialog.SetAddress(_deliveryAddress.Text);
                editShippingDialog.ShippingInfoChanged += (address, name, tel) =>
                {
                    _deliveryAddress.Text = address;
                    _checkoutButton.Enabled = CheckCheckout();
                };
                editShippingDialog.Show(transaction, "shipping_dialog");

            };


            return view;
        }

        public async override void OnStart()
        {
            base.OnStart();
            Title = "سبد خرید";
            AnatoliApp.GetInstance().HideMenuIcon();
            AnatoliApp.GetInstance().HideSearchIcon();

            try
            {
                var defaultStore = StoreManager.GetDefault();
                if (defaultStore != null)
                {
                    string tel = defaultStore.Phone;
                    if (String.IsNullOrEmpty(tel))
                    {
                        _storeTelTextView.Text = "نا مشخص";
                        _callImageButton.Visibility = ViewStates.Invisible;
                    }
                    else
                    {
                        _storeTelTextView.Text = tel;
                        _callImageButton.Visibility = ViewStates.Visible;
                        _callImageButton.Click += (s, e) =>
                        {
                            var uri = Android.Net.Uri.Parse(String.Format("tel:{0}", tel));
                            var intent = new Intent(Intent.ActionDial, uri);
                            StartActivity(intent);
                        };
                    }
                }
                else
                {
                    var storef = new StoresListFragment();
                    storef.SetQuery(StoreManager.GetAllQueryString());
                    AnatoliApp.GetInstance().PushFragment(new StoresListFragment(), "stores_fragment");
                }

                if (!AnatoliClient.GetInstance().WebClient.IsOnline())
                {
                    Toast.MakeText(Activity, "لطفا دستگاه خود را به منظور بروزرسانی اطلاعات به اینترنت متصل نمایید", ToastLength.Short).Show();
                }
                else
                {
                    var progressDialog = new ProgressDialog(Activity);
                    try
                    {
                        var latestUpdateTime = SyncManager.GetLog(SyncManager.StoreCalendarTbl);
                        if ((DateTime.Now - latestUpdateTime) > TimeSpan.FromMinutes(15))
                        {
                            progressDialog.SetMessage("بروزرسانی اطلاعات");
                            progressDialog.SetCancelable(false);
                            progressDialog.SetButton(Resources.GetString(Resource.String.Ok), delegate { progressDialog.Dismiss(); });
                            progressDialog.Show();
                            await StoreManager.SyncStoreCalendar();
                        }
                    }
                    catch (AnatoliWebClientException ex)
                    {
                        var alert = new AlertDialog.Builder(Activity);
                        alert.SetMessage(ex.MetaInfo.ModelStateString);
                        alert.SetPositiveButton(Resource.String.Ok, delegate { });
                        alert.Show();
                    }
                    catch (Exception ex)
                    {
                        var alert = new AlertDialog.Builder(Activity);
                        alert.SetMessage(Resource.String.ErrorOccured);
                        alert.SetPositiveButton(Resource.String.Ok, delegate { });
                        alert.Show();
                        ex.SendTrace();
                    }
                    finally
                    {
                        progressDialog.Dismiss();
                    }
                }


            }
            catch (Exception)
            {

            }

            try
            {
                _typeOptions = DeliveryTypeManager.GetDeliveryTypes();
                foreach (var item in _typeOptions)
                {
                    item.UniqueId = item.UniqueId;
                    _deliveryTypeListBox.AddItem(item);
                }
                _deliveryTypeListBox.ItemSelected += (item) =>
                {
                    _timeOptions = DeliveryTimeManager.GetAvailableDeliveryTimes(AnatoliApp.GetInstance().DefaultStore.UniqueId, DateTime.Now.ToLocalTime(), _deliveryTypeListBox.SelectedItem.UniqueId);
                    _deliveryTimeListBox.SetList(_timeOptions);
                    _deliveryTimeListBox.SelectItem(0);
                };
                _deliveryTimeListBox.ItemSelected += (item) => { _checkoutButton.Enabled = CheckCheckout(); };
                _deliveryTypeListBox.SelectItem(1);

            }
            catch (Exception)
            {

                throw;
            }

            var cardInfo = ShoppingCardManager.GetInfo();
            _factorePriceTextView.Text = cardInfo.TotalPrice.ToCurrency() + " تومان";
            _itemCountTextView.Text = cardInfo.Qty.ToString() + " عدد";
            _listAdapter = new ProductsListAdapter();
            _listAdapter.List = ShoppingCardManager.GetAllItems();
            _listAdapter.NotifyDataSetChanged();
            _listAdapter.DataChanged += (s) =>
            {
                var cardInfoDetail = ShoppingCardManager.GetInfo();

                _factorePriceTextView.Text = cardInfoDetail.TotalPrice.ToCurrency() + " تومان";
                _itemCountTextView.Text = cardInfoDetail.Qty.ToString() + " عدد";
                _checkoutButton.Enabled = CheckCheckout();
            };
            _listAdapter.ShoppingCardItemRemoved += (s, item) =>
            {
                _listAdapter.List.Remove(item);
                _itemsListView.InvalidateViews();
                _checkoutButton.Enabled = CheckCheckout();
                Toast.MakeText(AnatoliAndroid.Activities.AnatoliApp.GetInstance().Activity, "حذف شد", ToastLength.Short).Show();
            };
            _itemsListView.Adapter = _listAdapter;
            if (_listAdapter.Count == 0)
            {
                Toast.MakeText(AnatoliAndroid.Activities.AnatoliApp.GetInstance().Activity, "سبد خرید خالی است", ToastLength.Short).Show();
                _checkoutButton.Enabled = CheckCheckout();
            }

            await UpdateShippingInfo();

            _checkoutButton.Enabled = CheckCheckout();

            var cardInfoChange = ShoppingCardManager.GetInfo();

            _factorePriceTextView.Text = cardInfoChange.TotalPrice.ToCurrency() + " تومان";
            _countTextView.Text = cardInfoChange.Qty.ToString() + " عدد";


        }

        async Task<bool> UpdateShippingInfo()
        {
            _customerViewModel = AnatoliApp.GetInstance().Customer;
            if (_customerViewModel == null)
                _customerViewModel = await AnatoliApp.GetInstance().RefreshCutomerProfile();
            if (_customerViewModel != null)
            {
                _deliveryAddress.Text = _customerViewModel.MainStreet;
                if (String.IsNullOrEmpty(_customerViewModel.FirstName) || String.IsNullOrEmpty(_customerViewModel.LastName) || String.IsNullOrEmpty(_customerViewModel.NationalCode) || String.IsNullOrEmpty(_customerViewModel.MainStreet))
                {
                    AlertDialog.Builder lAlert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                    lAlert.SetMessage("لطفا مشخصات خود را کامل کنید");
                    lAlert.SetPositiveButton(Resource.String.Ok, delegate
                    {
                        var transaction = AnatoliApp.GetInstance().Activity.FragmentManager.BeginTransaction();
                        var profileFragment = new ProfileFragment();
                        profileFragment.ProfileUpdated += async () => { await UpdateShippingInfo(); };
                        profileFragment.Show(transaction, "profile_fragment");
                    });
                    lAlert.Show();
                    return false;
                }
                return true;
            }
            return false;
        }
        bool CheckCheckout()
        {
            if (_deliveryTimeListBox == null || _deliveryTypeListBox == null)
            {
                return false;
            }
            if (_deliveryTimeListBox.SelectedItem == null || _deliveryTypeListBox.SelectedItem == null || String.IsNullOrWhiteSpace(_deliveryAddress.Text) || String.IsNullOrEmpty(_deliveryAddress.Text) || _listAdapter.Count == 0)
                return false;
            else
                return true;
        }
    }
    public class OrderSavedDialogFragment : Android.Support.V4.App.DialogFragment
    {
        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            var builder = new AlertDialog.Builder(Activity)
                .SetMessage("سفارش شما با موفقیت ثبت گردید. برای اطلاع از وضعیت سفارش خود به بخش پیغام ها یا سفارشات قبلی مراجعه نمایید")
                .SetPositiveButton("باشه", delegate
                {
                })
                .SetTitle("ثبت سفارش");
            return builder.Create();
        }
    }
}