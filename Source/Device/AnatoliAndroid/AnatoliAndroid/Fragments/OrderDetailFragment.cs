﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Anatoli.App.Model.Store;
using Anatoli.App.Manager;
using AnatoliAndroid.Activities;
using System.Threading.Tasks;
using Square.Picasso;
using Anatoli.Framework.AnatoliBase;
using System.Globalization;

namespace AnatoliAndroid.Fragments
{
    public class OrderDetailFragment : AnatoliFragment
    {
        ListView _itemsListView;
        TextView _dateTextView;
        TextView _storeNameTextView;
        TextView _priceTextView;
        TextView _orderIdTextView;
        TextView _orderStatusTextView;
        Button _addAllButton;
        string _appOrderNumber;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.OrderViewLayout, null);
            _dateTextView = view.FindViewById<TextView>(Resource.Id.dateTextView);
            _storeNameTextView = view.FindViewById<TextView>(Resource.Id.storeNameTextView);
            _priceTextView = view.FindViewById<TextView>(Resource.Id.priceTextView);
            _orderIdTextView = view.FindViewById<TextView>(Resource.Id.orderNoTextView);
            _orderStatusTextView = view.FindViewById<TextView>(Resource.Id.orderStatusTextView);
            _itemsListView = view.FindViewById<ListView>(Resource.Id.itemsListView);
            _addAllButton = view.FindViewById<Button>(Resource.Id.addAllButton);
            _addAllButton.UpdateWidth();
            try
            {
                _appOrderNumber = Arguments.GetString("AppOrderNo");
            }
            catch (Exception)
            {
                AnatoliApp.GetInstance().BackFragment();
                _appOrderNumber = "-1";
            }
            return view;
        }
        public async override void OnStart()
        {
            base.OnStart();
            Title = "سفارش ها";
            if (_appOrderNumber.Equals("-1"))
            {
                AnatoliApp.GetInstance().BackFragment();
            }
            else
            {
                PurchaseOrderViewModel order = PurchaseOrderManager.GetOrderByAppOrderNumber(long.Parse(_appOrderNumber));
                _dateTextView.Text = " " + order.OrderPDate;
                _storeNameTextView.Text = " " + order.StoreName;
                _priceTextView.Text = " " + order.FinalNetAmount.ToCurrency() + " تومان";
                _orderIdTextView.Text = order.AppOrderNo.ToString();
                _orderStatusTextView.Text = PurchaseOrderStatusHistoryViewModel.GetStatusName(order.PurchaseOrderStatusValueId);


                if (AnatoliClient.GetInstance().WebClient.IsOnline())
                {
                    var pDialog = new ProgressDialog(AnatoliApp.GetInstance().Activity);
                    pDialog.SetMessage("در حال بروزرسانی اطلاعات");
                    pDialog.SetButton("بی خیال", delegate
                    {
                        AnatoliApp.GetInstance().BackFragment();
                    });
                    pDialog.Show();
                    await Task.Delay(1000);
                    try
                    {
                        await Task.Run(async delegate
                        {
                            await PurchaseOrderManager.SyncOrderItemsAsync(AnatoliApp.GetInstance().Customer.UniqueId, order);
                        });
                    }
                    catch (Exception ex)
                    {
                        pDialog.Dismiss();
                        ex.SendTrace();
                    }
                    finally
                    {
                        pDialog.Dismiss();
                    }
                }
                else
                {
                    var alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                    alert.SetMessage("برای اطلاع از آخرین وضعیت سفارش دستگاه خود را به اینترنت متصل نمایید.");
                    alert.SetPositiveButton(Resource.String.Ok, delegate { });
                    alert.Show();
                }


                List<PurchaseOrderLineItemViewModel> items = OrderItemsManager.GetItems(order.UniqueId);
                OrderDetailAdapter adapter = new OrderDetailAdapter(items, order, AnatoliApp.GetInstance().Activity);
                adapter.DataChanged += (s, e) =>
                {
                    _itemsListView.InvalidateViews();
                };
                _itemsListView.Adapter = adapter;

                _addAllButton.Click += async (s, e) =>
                {
                    if (items != null)
                    {
                        decimal a = 0;
                        foreach (var item in items)
                        {
                            await Task.Delay(100);
                            if (ShoppingCardManager.AddProduct(item.UniqueId, order.StoreGuid, 1))
                            {
                                a += item.FinalQty;
                            }

                        }
                        if (a > 0)
                        {
                            Toast.MakeText(AnatoliApp.GetInstance().Activity, String.Format("{0} آیتم به سبد خرید اضافه شد", a.ToString()), ToastLength.Short).Show();
                            AnatoliApp.GetInstance().PushFragment(new ShoppingCardFragment(), "shopping_fragment");
                        }
                        else
                            Toast.MakeText(AnatoliApp.GetInstance().Activity, "کالای مورد نظر هم اکنون در دسترس نمی باشد", ToastLength.Short).Show();
                    }
                };
            }
        }

        class OrderDetailAdapter : BaseAdapter<PurchaseOrderLineItemViewModel>
        {
            List<PurchaseOrderLineItemViewModel> items;
            Activity _context;
            PurchaseOrderViewModel _order;
            public OrderDetailAdapter(List<PurchaseOrderLineItemViewModel> items, PurchaseOrderViewModel order, Activity context)
            {
                this.items = items;
                _context = context;
                _order = order;
            }
            public override PurchaseOrderLineItemViewModel this[int position]
            {
                get
                {
                    return items[position];
                }
            }

            public override int Count
            {
                get
                {
                    return items.Count;
                }
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                convertView = _context.LayoutInflater.Inflate(Resource.Layout.OrderItemModelLayout, null);
                PurchaseOrderLineItemViewModel item = null;
                if (items != null)
                    item = items[position];
                else
                    return convertView;
                TextView productCountTextView = convertView.FindViewById<TextView>(Resource.Id.productCountTextView);
                TextView productPriceTextView = convertView.FindViewById<TextView>(Resource.Id.productPriceTextView);
                TextView productNameTextView = convertView.FindViewById<TextView>(Resource.Id.productNameTextView);
                ImageView addProductImageView = convertView.FindViewById<ImageView>(Resource.Id.addProductImageView);
                ImageView productSummaryImageView = convertView.FindViewById<ImageView>(Resource.Id.productSummaryImageView);
                if (!String.IsNullOrEmpty(item.ProductImage))
                {
                    string imguri = ProductManager.GetImageAddress(item.ProductId, item.ProductImage);
                    if (imguri != null)
                        Picasso.With(AnatoliApp.GetInstance().Activity).Load(imguri).Placeholder(Resource.Drawable.igmart).Into(productSummaryImageView);
                    else
                        productSummaryImageView.Visibility = ViewStates.Invisible;
                }
                else
                {
                    productSummaryImageView.SetImageResource(Resource.Drawable.igmart);
                }

                //ImageView addToFavoritsImageView = convertView.FindViewById<ImageView>(Resource.Id.addToFavoritsImageView);
                //if (item.IsFavorit)
                //    addToFavoritsImageView.SetImageResource(Resource.Drawable.ic_assignment_white_24dp);
                //else
                //    addToFavoritsImageView.SetImageResource(Resource.Drawable.ic_assignment_white_24dp);
                productPriceTextView.Text = " (" + item.FinalNetAmount.ToCurrency() + " تومان) ";
                productCountTextView.Text = item.FinalQty.ToString();
                productNameTextView.Text = item.ProductName;
                addProductImageView.Click += (s, e) =>
                {
                    if (ShoppingCardManager.AddProduct(item.ProductId, AnatoliApp.GetInstance().DefaultStore.UniqueId, 1))
                    {

                        Toast.MakeText(_context, "به سبد خرید اضافه شد", ToastLength.Short).Show();
                    }
                    else
                        Toast.MakeText(_context, "کالای مورد نظر هم اکنون در دسترس نمی باشد", ToastLength.Short).Show();
                };
                //addToFavoritsImageView.Click += async (s, e) =>
                //{
                //    if (item.IsFavorit)
                //    {
                //        if (await ProductManager.RemoveFavorit(this[position].product_id) == true)
                //        {
                //            this[position].favorit = 0;
                //            NotifyDataSetChanged();
                //            OnDataChanged();
                //        }
                //    }
                //    else
                //    {
                //        if (await ProductManager.AddToFavorits(this[position].product_id) == true)
                //        {
                //            this[position].favorit = 1;
                //            NotifyDataSetChanged();
                //            OnDataChanged();
                //        }
                //    }
                //};
                return convertView;
            }

            void OnDataChanged()
            {
                if (DataChanged != null)
                {
                    DataChanged.Invoke(this, new EventArgs());
                }
            }

            public event EventHandler DataChanged;
        }
    }
}