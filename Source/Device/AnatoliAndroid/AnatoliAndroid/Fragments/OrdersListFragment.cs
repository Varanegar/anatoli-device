using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Anatoli.App.Manager;
using Anatoli.App.Model.Store;
using AnatoliAndroid.ListAdapters;
using Anatoli.Framework.AnatoliBase;
using AnatoliAndroid.Activities;

namespace AnatoliAndroid.Fragments
{
    class OrdersListFragment : BaseListFragment<OrderManager, OrdersListAdapter, OrderModel>
    {
        public OrdersListFragment()
        {
            StringQuery query = OrderManager.GetOrderQueryString();
            _dataManager.SetQueries(query, null);
        }
        public override void OnResume()
        {
            base.OnResume();
            EmptyList += (s, e) =>
            {
                if (_listAdapter.List.Count == 0)
                {
                    Toast.MakeText(AnatoliAndroid.Activities.AnatoliApp.GetInstance().Activity, "هیچ سفارشی ثبت نشده است", ToastLength.Short).Show();
                }
            };
        }
        public override void OnStart()
        {
            base.OnStart();
            AnatoliApp.GetInstance().HideSearchIcon();
        }
    }
}