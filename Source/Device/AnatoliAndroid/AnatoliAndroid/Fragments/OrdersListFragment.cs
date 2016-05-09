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
        public async override void OnResume()
        {
            base.OnResume();
            EmptyList += (s, e) =>
            {
                if (_listAdapter.List.Count == 0)
                {
                    Toast.MakeText(AnatoliAndroid.Activities.AnatoliApp.GetInstance().Activity, "هیچ سفارشی ثبت نشده است", ToastLength.Short).Show();
                }
            };
            if (AnatoliClient.GetInstance().WebClient.IsOnline())
            {
                ProgressDialog progressDialog = new ProgressDialog(Activity);
                try
                {
                    progressDialog.SetMessage("در حال دریافت آخرین وضعیت سفارشات");
                    progressDialog.SetButton("باشه", delegate { progressDialog.Dismiss(); });
                    progressDialog.Show();
                    await OrderManager.SyncOrdersAsync(AnatoliApp.GetInstance().CustomerId);
                    await RefreshAsync();
                }
                catch (Exception ex)
                {
                    ex.SendTrace();
                }
                finally
                {
                    progressDialog.Dismiss();
                }
            }
            else
            {
                var alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                alert.SetMessage("برای اطلاع از آخرین وضعیت سفارش ها دستگاه خود را به اینترنت متصل نمایید.");
                alert.SetPositiveButton(Resource.String.Ok, delegate { });
                alert.Show();
            }
        }
        public override void OnStart()
        {
            base.OnStart();
            AnatoliApp.GetInstance().HideSearchIcon();
        }
    }
}