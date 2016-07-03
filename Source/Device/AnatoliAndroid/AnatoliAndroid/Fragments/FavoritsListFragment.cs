using System;
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
using Anatoli.App.Manager;
using AnatoliAndroid.ListAdapters;
using Anatoli.Framework.AnatoliBase;
using Anatoli.Framework.Manager;
using AnatoliAndroid.Activities;

namespace AnatoliAndroid.Fragments
{
    class FavoritsListFragment : BaseListFragment<ProductManager, ProductsListAdapter, ProductModel>
    {
        public override void OnStart()
        {
            base.OnStart();
            AnatoliApp.GetInstance().HideSearchIcon();
            Title = "فهرست من";
        }
        public FavoritsListFragment()
        {
            _listAdapter.FavoritRemoved += _listAdapter_FavoritRemoved;
        }


        void _listAdapter_FavoritRemoved(object sender, ProductModel item)
        {
            _listAdapter.List.Remove(item);
            _listView.InvalidateViews();
        }


    }
}