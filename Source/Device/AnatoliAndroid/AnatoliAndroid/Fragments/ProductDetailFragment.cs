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
using Anatoli.App.Model.Product;
using Anatoli.Framework.AnatoliBase;
using AnatoliAndroid.Activities;
using Anatoli.App.Manager;
using Square.Picasso;

namespace AnatoliAndroid.Fragments
{
    public class ProductDetailFragment : AnatoliFragment
    {
        ProductModel _product;
        public ProductDetailFragment(ProductModel product)
        {
            _product = product;
        }
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            var view = inflater.Inflate(Resource.Layout.ProductDetailLayout, container, false);

            var addImageButton = view.FindViewById<ImageButton>(Resource.Id.addImageButton);
            var titleTextView = view.FindViewById<TextView>(Resource.Id.titleTextView);
            var priceTextView = view.FindViewById<TextView>(Resource.Id.priceTextView);
            var producerTextView = view.FindViewById<TextView>(Resource.Id.producerTextView);
            var productGroupTextView = view.FindViewById<TextView>(Resource.Id.productGroupTextView);
            var tagsTextView = view.FindViewById<TextView>(Resource.Id.tagsTextView);
            var counterLinearLayout = view.FindViewById<LinearLayout>(Resource.Id.counterLinearLayout);
            var removeProductImageView = view.FindViewById<ImageButton>(Resource.Id.removeProductImageView);
            var productCountTextView = view.FindViewById<TextView>(Resource.Id.productCountTextView);
            var productImageView = view.FindViewById<ImageView>(Resource.Id.productImageView);
            var favoritImageView = view.FindViewById<ImageView>(Resource.Id.favoritImageView);

            titleTextView.Text = _product.StoreProductName;
            priceTextView.Text = (_product.IsAvailable) ? _product.Price.ToCurrency() + " تومان" : "موجود نیست";
            productCountTextView.Text = _product.ShoppingBasketCount.ToString() + " عدد";
            productGroupTextView.Text = _product.GroupName;
            Picasso.With(AnatoliApp.GetInstance().Activity).Load(ProductManager.GetImageAddress(_product.UniqueId, _product.ImageAddress)).Placeholder(Resource.Drawable.igmart).Into(productImageView);
            if (_product.IsFavorit)
                favoritImageView.SetImageResource(Android.Resource.Drawable.ButtonStarBigOn);
            else
                favoritImageView.SetImageResource(Android.Resource.Drawable.ButtonStarBigOff);

            favoritImageView.Click += delegate
            {
                if (!_product.IsFavorit)
                {
                    var result = ProductManager.AddToFavorits(_product);
                    if (result)
                    {
                        favoritImageView.SetImageResource(Android.Resource.Drawable.ButtonStarBigOn);
                    }
                }
                else
                {
                    var result = ProductManager.RemoveFavorit(_product);
                    if (result)
                    {
                        favoritImageView.SetImageResource(Android.Resource.Drawable.ButtonStarBigOff);
                    }
                }
            };
            addImageButton.Click += delegate
            {
                try
                {
                    if (_product.ShoppingBasketCount + 1 > _product.Qty)
                    {
                        AlertDialog.Builder alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                        alert.SetMessage("موجودی کالا کافی نیست");
                        alert.SetPositiveButton("باشه", delegate { });
                        alert.Show();
                        return;
                    }
                    addImageButton.Enabled = false;
                    if (AnatoliApp.GetInstance().AnatoliUser != null)
                    {
                        if (ShoppingCardManager.AddProduct(_product))
                        {
                            if (_product.ShoppingBasketCount == 1)
                            {
                                counterLinearLayout.Visibility = ViewStates.Visible;
                            }
                            productCountTextView.Text = _product.ShoppingBasketCount.ToString() + " عدد";
                            
                        }

                    }
                    else
                    {
                        Toast.MakeText(AnatoliApp.GetInstance().Activity, Resource.String.PleaseLogin, ToastLength.Short).Show();
                        LoginFragment login = new LoginFragment();
                        var transaction = AnatoliApp.GetInstance().Activity.FragmentManager.BeginTransaction();
                        login.Show(transaction, "login_dialog");
                    }
                    addImageButton.Enabled = true;
                }
                catch (System.Exception)
                {

                }
                finally
                {
                    addImageButton.Enabled = true;
                }
            };

            removeProductImageView.Click += delegate
            {
                try
                {
                    removeProductImageView.Enabled = false;
                    if (AnatoliApp.GetInstance().AnatoliUser != null)
                    {
                        if (ShoppingCardManager.RemoveProduct(_product))
                        {
                            if (_product.ShoppingBasketCount == 0)
                            {
                                counterLinearLayout.Visibility = ViewStates.Gone;
                            }
                            productCountTextView.Text = _product.ShoppingBasketCount.ToString() + " عدد";
                            
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
                    removeProductImageView.Enabled = true;
                }
            };
            return view;
        }
    }
}