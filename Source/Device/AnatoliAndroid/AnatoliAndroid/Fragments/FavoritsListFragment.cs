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
using Anatoli.Framework.DataAdapter;
using AnatoliAndroid.Activities;

namespace AnatoliAndroid.Fragments
{
    [FragmentTitle("فهرست من")]
    class FavoritsListFragment : BaseListFragment<ProductManager, ProductsListAdapter, NoListToolsDialog, ProductModel>
    {
        public override void OnStart()
        {
            base.OnStart();
            AnatoliApp.GetInstance().HideSearchIcon();
        }
        public FavoritsListFragment()
        {
            _listAdapter.FavoritRemoved += _listAdapter_FavoritRemoved;
            var query = ProductManager.GetFavoritsQueryString(AnatoliApp.GetInstance().DefaultStoreId);
            _dataManager.SetQueries(query, null);
        }
        void _toolsFragment_FavoritsRemoved()
        {
            _listAdapter.List.RemoveAll((p) => { return true; });
            _listView.InvalidateViews();
            Toast.MakeText(AnatoliApp.GetInstance().Activity, "لیست علاقه مندی ها پاک شد", ToastLength.Short);
        }

        void _listAdapter_FavoritRemoved(object sender, ProductModel item)
        {
            _listAdapter.List.Remove(item);
            _listView.InvalidateViews();
        }


    }
}