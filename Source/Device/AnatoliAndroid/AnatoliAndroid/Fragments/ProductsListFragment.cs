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
using AnatoliAndroid.Activities;
using System.Threading.Tasks;

namespace AnatoliAndroid.Fragments
{
    class ProductsListFragment : BaseListFragment<ProductManager, ProductsListAdapter, ProductModel>
    {
        Guid? _catId;
        public override void OnStart()
        {
            base.OnStart();
            Title = "دسته بندی کالا";
            AnatoliApp.GetInstance().ShowSearchIcon();
            if (_catId != null)
            {
                AnatoliApp.GetInstance().RefreshMenuItems(_catId);
                var catInfo = ProductGroupManager.GetGroupInfo((Guid)_catId);
                if (catInfo != null)
                {
                    Title = catInfo.GroupName;
                }
            }
            else
                AnatoliApp.GetInstance().RefreshMenuItems();
        }

        public void Search(DBQuery query, string value)
        {
            //_dataManager.ShowGroups = true;
            //_dataManager.Query = query;
            //try
            //{
            //    _listAdapter.List = _dataManager.GetNext();
            //    Title = string.Format("جستجو  \"{0}\"", value.Trim());
            //}
            //catch (Exception ex)
            //{
            //    ex.SendTrace();
            //}

            //var groups = ProductGroupManager.Search(value);
            //List<ProductModel> pl = new List<ProductModel>();
            //foreach (var item in groups)
            //{
            //    var p = new ProductModel();
            //    p.cat_id = item.cat_id;
            //    p.product_name = item.cat_name;
            //    p.is_group = 1;
            //    p.message = "group";
            //    p.image = ProductGroupManager.GetGroupInfo(item.UniqueId).Image;
            //    pl.Add(p);
            //}
            //_listAdapter.List.InsertRange(0, pl);
            //if (_listAdapter.List.Count > 0)
            //    OnFullList();
            //else
            //    OnEmptyList();
        }
        public void SetCatId(Guid id)
        {
            try
            {
                _dataManager.ShowGroups = false;
                var query = ProductManager.GetGroupQueryString(id, AnatoliApp.GetInstance().DefaultStore.UniqueId);
                query.Unlimited = false;
                _dataManager.Query = query;
                _catId = id;
            }
            catch (Exception ex)
            {
                ex.SendTrace();
                return;
            }
        }
        public void UnSetCatId()
        {
            try
            {
                _dataManager.ShowGroups = true;
                var query = ProductManager.GetAll(AnatoliApp.GetInstance().DefaultStore.UniqueId);
                _dataManager.Query = query;
                _catId = null;
            }
            catch (Exception ex)
            {
                ex.SendTrace();
                return;
            }
        }
    }
}