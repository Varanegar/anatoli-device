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
using Anatoli.App.Model.AnatoliUser;
using Anatoli.App.Manager;
using System.Threading.Tasks;
using AnatoliAndroid.Activities;
using AnatoliAndroid.Fragments;
using Android.Content.Res;
using Java.Lang;
using AnatoliAndroid.Components;
using Anatoli.Framework.AnatoliBase;
using Square.Picasso;

namespace AnatoliAndroid.ListAdapters
{
    class ProductsListAdapter : BaseListAdapter<ProductManager, ProductModel>
    {
        TextView _productCountTextView;
        TextView _productNameTextView;
        TextView _productPriceTextView;
        TextView _favoritsTextView;
        TextView _removeFromBasketTextView;

        ImageView _productIimageView;
        ImageButton _productAddButton;
        ImageButton _favoritsButton;
        ImageButton _productRemoveButton;
        ImageButton _removeAllProductsButton;
        OnTouchListener _addTouchlistener;
        LinearLayout _counterLinearLayout;
        RelativeLayout _removeAllRelativeLayout;
        ImageView _groupImageView;
        TextView _groupNameTextView;
        LinearLayout _optionslinearLayout;
        public override View GetItemView(int position, View convertView, ViewGroup parent)
        {

            View view = null;
            ProductModel item = null;
            if (List != null)
                item = this[position];
            else
                return view;

            if (item.IsGroup)
                if (!string.IsNullOrEmpty(item.message) && item.message.Equals("group"))
                    view = _context.LayoutInflater.Inflate(Resource.Layout.GroupDetailLayout, null);
                else
                    view = _context.LayoutInflater.Inflate(Resource.Layout.GroupSummaryLayout, null);
            else
                view = _context.LayoutInflater.Inflate(Resource.Layout.ProductSummaryLayout, null);

            if (item.IsGroup)
            {
                _groupNameTextView = view.FindViewById<TextView>(Resource.Id.textView1);
                _groupImageView = view.FindViewById<ImageView>(Resource.Id.groupImageView);
            }
            else
            {
                _productNameTextView = view.FindViewById<TextView>(Resource.Id.productNameTextView);
                _favoritsTextView = view.FindViewById<TextView>(Resource.Id.favoritsTextView);
                _removeFromBasketTextView = view.FindViewById<TextView>(Resource.Id.removeFromBasketTextView);
                _productPriceTextView = view.FindViewById<TextView>(Resource.Id.productPriceTextView);
                _productCountTextView = view.FindViewById<TextView>(Resource.Id.productCountTextView);
                _productIimageView = view.FindViewById<ImageView>(Resource.Id.productSummaryImageView);
                _productAddButton = view.FindViewById<ImageButton>(Resource.Id.addImageButton);
                _productRemoveButton = view.FindViewById<ImageButton>(Resource.Id.removeProductImageView);
                _removeAllProductsButton = view.FindViewById<ImageButton>(Resource.Id.removeAllProductsButton);
                _removeAllRelativeLayout = view.FindViewById<RelativeLayout>(Resource.Id.removeAllRelativeLayout);
                _favoritsButton = view.FindViewById<ImageButton>(Resource.Id.favoritsButton);
                _counterLinearLayout = view.FindViewById<LinearLayout>(Resource.Id.counterLinearLayout);
                _optionslinearLayout = view.FindViewById<LinearLayout>(Resource.Id.optionslinearLayout);
            }
            if (item.IsGroup)
            {
                if (!string.IsNullOrEmpty(item.message) && item.message.Equals("group"))
                {
                    var imguriii = ProductGroupManager.GetImageAddress(item.cat_id, item.image);
                    if (imguriii != null)
                    {
                        Picasso.With(AnatoliApp.GetInstance().Activity).Load(imguriii).Placeholder(Resource.Drawable.igmart).Into(_groupImageView);
                    }
                    else
                    {
                        _groupImageView.Visibility = ViewStates.Invisible;
                    }
                }
                _groupNameTextView.Text = item.StoreProductName;
                _groupNameTextView.Click += (s, e) =>
                {
                    var p = new ProductsListFragment();
                    p.SetCatId((Guid)item.ProductGroupId);
                    AnatoliApp.GetInstance().PushFragment(p, "products_fragment", true);
                };
                if (_groupImageView != null)
                {
                    _groupImageView.Click += (s, e) =>
                    {
                        var p = new ProductsListFragment();
                        p.SetCatId((Guid)item.ProductGroupId);
                        AnatoliApp.GetInstance().PushFragment(p, "products_fragment", true);
                    };
                }
                return view;
            }
            else
            {
                string imguri = ProductManager.GetImageAddress(item.UniqueId, item.ImageAddress);
                if (imguri != null)
                {
                    Picasso.With(AnatoliApp.GetInstance().Activity).Load(imguri).Placeholder(Resource.Drawable.igmart).Into(_productIimageView);
                }

                if (item.IsFavorit)
                {
                    _favoritsTextView.Text = _context.Resources.GetText(Resource.String.RemoveFromList);
                    _favoritsButton.SetImageResource(Resource.Drawable.ic_mylist_orange_24dp);
                    _favoritsTextView.SetTextColor(Android.Graphics.Color.Red);
                }
                else
                {
                    _favoritsTextView.Text = _context.Resources.GetText(Resource.String.AddToList);
                    _favoritsTextView.SetTextColor(Android.Graphics.Color.DarkGreen);
                    _favoritsButton.SetImageResource(Resource.Drawable.ic_mylist_green_24dp);
                }

                _productCountTextView.Text = item.ShoppingBasketCount.ToString() + " عدد";
                _productNameTextView.Text = item.StoreProductName;
                if (item.IsAvailable)
                {
                    _productPriceTextView.Text = string.Format(" {0} تومان", item.Price.ToCurrency());
                    _productIimageView.Alpha = 1f;
                    _productAddButton.Alpha = 1f;
                    _productNameTextView.Alpha = 1f;
                }
                else
                {
                    _productIimageView.Alpha = .4f;
                    _productAddButton.Alpha = .4f;
                    _productNameTextView.Alpha = 0.4f;
                    _productPriceTextView.Text = "موجود نیست";
                    _productAddButton.Enabled = false;
                }

                if (item.StoreProductName.Equals(_productNameTextView.Text))
                {
                    if (item.ShoppingBasketCount > 0)
                    {
                        _counterLinearLayout.Visibility = ViewStates.Visible;
                        _removeAllRelativeLayout.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        _counterLinearLayout.Visibility = ViewStates.Gone;
                        _removeAllRelativeLayout.Visibility = ViewStates.Invisible;
                    }
                }


                var removeAll = new OnTouchListener();
                _removeFromBasketTextView.SetOnTouchListener(removeAll);
                removeAll.Click += async (s, e) =>
                {
                    try
                    {
                        _removeAllProductsButton.Enabled = false;
                        if (AnatoliApp.GetInstance().AnatoliUser != null)
                        {
                            var cardInfoChange = ShoppingCardManager.GetInfo();

                            int a = cardInfoChange.Qty;

                            if (ShoppingCardManager.RemoveProduct(item, true))
                            {
                                while (item.Qty > 0)
                                {
                                    await Task.Delay(90);
                                    item.Qty--;
                                    NotifyDataSetChanged();
                                    OnDataChanged();
                                }
                                if (item.StoreProductName.Equals(_productNameTextView.Text))
                                {
                                    _counterLinearLayout.Visibility = ViewStates.Gone;
                                    _removeAllRelativeLayout.Visibility = ViewStates.Invisible;
                                }
                                NotifyDataSetChanged();
                                OnDataChanged();
                                OnShoppingCardItemRemoved(item);
                                Toast.MakeText(AnatoliApp.GetInstance().Activity, Resource.String.ItemRemoved, ToastLength.Short).Show();
                            }

                        }
                        else
                        {
                            Toast.MakeText(AnatoliApp.GetInstance().Activity, Resource.String.PleaseLogin, ToastLength.Short).Show();
                            LoginFragment login = new LoginFragment();
                            var transaction = AnatoliApp.GetInstance().Activity.FragmentManager.BeginTransaction();
                            login.Show(transaction, "login_dialog");
                        }
                    }
                    catch (System.Exception)
                    {

                    }
                    finally
                    {
                        _removeAllProductsButton.Enabled = true;
                    }
                };
                _removeAllProductsButton.SetOnTouchListener(removeAll);


                var _favoritsTouchlistener = new OnTouchListener();
                _favoritsTextView.SetOnTouchListener(_favoritsTouchlistener);
                _favoritsTouchlistener.Click += (s, e) =>
                {
                    _favoritsButton.Enabled = false;
                    if (this[position].IsFavorit)
                    {
                        if (ProductManager.RemoveFavorit(this[position]) == true)
                        {
                            this[position].FavoritBasketCount = 0;
                            NotifyDataSetChanged();
                            OnFavoritRemoved(this[position]);
                        }
                    }
                    else
                    {
                        if (ProductManager.AddToFavorits(this[position]) == true)
                        {
                            this[position].FavoritBasketCount = 1;
                            NotifyDataSetChanged();
                            OnFavoritAdded(this[position]);
                        }
                    }
                    _favoritsButton.Enabled = true;
                };
                _favoritsButton.SetOnTouchListener(_favoritsTouchlistener);

                AlertDialog.Builder alert1 = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                alert1.SetMessage("موجودی کالا کافی نیست");
                alert1.SetPositiveButton(Resource.String.Ok, delegate { });
                alert1.SetCancelable(false);
                _addTouchlistener = new OnTouchListener();
                _addTouchlistener.Click += (s, e) =>
                {
                    try
                    {
                        if (item.ShoppingBasketCount + 1 > item.Qty)
                        {
                            alert1.Show();
                            return;
                        }
                        _productAddButton.Enabled = false;
                        if (AnatoliApp.GetInstance().AnatoliUser != null)
                        {
                            if (ShoppingCardManager.AddProduct(item))
                            {
                                if (item.StoreProductName.Equals(_productNameTextView.Text))
                                    if (item.ShoppingBasketCount == 1)
                                    {
                                        _counterLinearLayout.Visibility = ViewStates.Visible;
                                        _removeAllRelativeLayout.Visibility = ViewStates.Visible;
                                    }
                                NotifyDataSetChanged();
                                OnDataChanged();

                            }

                        }
                        else
                        {
                            Toast.MakeText(AnatoliApp.GetInstance().Activity, Resource.String.PleaseLogin, ToastLength.Short).Show();
                            LoginFragment login = new LoginFragment();
                            var transaction = AnatoliApp.GetInstance().Activity.FragmentManager.BeginTransaction();
                            login.Show(transaction, "login_dialog");
                        }
                        _productAddButton.Enabled = true;
                    }
                    catch (System.Exception)
                    {

                    }
                    finally
                    {
                        _productAddButton.Enabled = true;
                    }
                };
                _productAddButton.SetOnTouchListener(_addTouchlistener);

                var _removeTouchlistener = new OnTouchListener();
                _removeTouchlistener.Click += (s, e) =>
                {
                    try
                    {
                        _productRemoveButton.Enabled = false;
                        if (AnatoliApp.GetInstance().AnatoliUser != null)
                        {
                            if (ShoppingCardManager.RemoveProduct(item))
                            {
                                if (item.ShoppingBasketCount == 0)
                                {
                                    if (item.StoreProductName.Equals(_productNameTextView.Text))
                                    {
                                        _counterLinearLayout.Visibility = ViewStates.Gone;
                                        _removeAllRelativeLayout.Visibility = ViewStates.Invisible;
                                    }
                                    OnShoppingCardItemRemoved(item);
                                }
                                NotifyDataSetChanged();
                                OnDataChanged();

                            }
                        }
                        else
                        {
                            Toast.MakeText(AnatoliApp.GetInstance().Activity, Resource.String.PleaseLogin, ToastLength.Short).Show();
                            LoginFragment login = new LoginFragment();
                            var transaction = AnatoliApp.GetInstance().Activity.FragmentManager.BeginTransaction();
                            login.Show(transaction, "login_dialog");
                        }
                    }
                    catch (System.Exception)
                    {

                    }
                    finally
                    {
                        _productRemoveButton.Enabled = true;
                    }
                };
                _productRemoveButton.SetOnTouchListener(_removeTouchlistener);
                _productIimageView.Click += delegate
                {
                    var d = new ProductDetailFragment(item);
                    AnatoliApp.GetInstance().PushFragment(d, "product_detail");
                };
                _productNameTextView.Click += delegate
                {
                    var d = new ProductDetailFragment(item);
                    AnatoliApp.GetInstance().PushFragment(d, "product_detail");
                };
                _productPriceTextView.Click += delegate
                {
                    var d = new ProductDetailFragment(item);
                    AnatoliApp.GetInstance().PushFragment(d, "product_detail");
                };
                return view;
            }
        }


        void OnShoppingCardItemRemoved(ProductModel data)
        {
            if (ShoppingCardItemRemoved != null)
            {
                ShoppingCardItemRemoved(this, data);
            }
        }
        public event ItemRemovedEventHandler ShoppingCardItemRemoved;
        public delegate void ItemRemovedEventHandler(object sender, ProductModel data);

        void OnShoppingCardItemAdded(ProductModel data)
        {
            if (ShoppingCardItemAdded != null)
            {
                ShoppingCardItemAdded(this, data);
            }
        }
        public event ItemAddedEventHandler ShoppingCardItemAdded;
        public delegate void ItemAddedEventHandler(object sender, ProductModel data);

        void OnFavoritRemoved(ProductModel data)
        {
            if (FavoritRemoved != null)
            {
                FavoritRemoved(this, data);
            }
        }
        public event FavoritRemovedEventHandler FavoritRemoved;
        public delegate void FavoritRemovedEventHandler(object sender, ProductModel data);

        void OnFavoritAdded(ProductModel data)
        {
            if (FavoritAdded != null)
            {
                FavoritAdded(this, data);
            }
        }
        public event FavoritAddedEventHandler FavoritAdded;
        public delegate void FavoritAddedEventHandler(object sender, ProductModel data);

    }


}