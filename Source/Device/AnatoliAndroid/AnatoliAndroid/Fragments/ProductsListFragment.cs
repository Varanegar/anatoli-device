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
using Anatoli.App.Manager;
using AnatoliAndroid.ListAdapters;
using Anatoli.Framework.AnatoliBase;
using AnatoliAndroid.Activities;
using System.Threading.Tasks;
using Anatoli.Framework.DataAdapter;
using Android.Animation;
using Android.Views.Animations;
using Anatoli.Framework;

namespace AnatoliAndroid.Fragments
{
    [FragmentTitle("دسته بندی کالا")]
    class ProductsListFragment : BaseListFragment<ProductManager, ProductsListAdapter, ProductModel>
    {
        public override void OnStart()
        {
            base.OnStart();
            AnatoliApp.GetInstance().ShowSearchIcon();
        }

        public async Task Search(DBQuery query, string value)
        {
            _dataManager.ShowGroups = true;
            _dataManager.SetQueries(query, null);
            try
            {
                _listAdapter.List = await _dataManager.GetNextAsync();
                AnatoliApp.GetInstance().SetToolbarTitle(string.Format("جستجو  \"{0}\"", value.Trim()));
            }
            catch (Exception ex)
            {
                ex.SendTrace();
            }

            var groups = await CategoryManager.SearchAsync(value);
            List<ProductModel> pl = new List<ProductModel>();
            foreach (var item in groups)
            {
                var p = new ProductModel();
                p.cat_id = item.cat_id;
                p.product_name = item.cat_name;
                p.is_group = 1;
                p.message = "group";
                p.image = (await CategoryManager.GetCategoryInfoAsync(item.cat_id)).cat_image;
                pl.Add(p);
            }
            _listAdapter.List.InsertRange(0, pl);
            if (_listAdapter.List.Count > 0)
                OnFullList();
            else
                OnEmptyList();
        }
        public async Task SetCatIdAsync(string id)
        {
            try
            {
                _dataManager.ShowGroups = false;
                var query = ProductManager.SetCatId(id, AnatoliApp.GetInstance().DefaultStoreId);
                _dataManager.SetQueries(query, null);
                Title = (await CategoryManager.GetCategoryInfoAsync(id)).cat_name;
            }
            catch (Exception ex)
            {
                ex.SendTrace();
                return;
            }
        }
    }
}