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
using AnatoliAndroid.Activities;
using AnatoliAndroid.Fragments;
using Anatoli.Framework.AnatoliBase;
using System.Threading.Tasks;

namespace AnatoliAndroid.ListAdapters
{
    class OrdersListAdapter : BaseListAdapter<PurchaseOrderManager, PurchaseOrderViewModel>
    {
        public override View GetItemView(int position, View convertView, ViewGroup parent)
        {
            convertView = _context.LayoutInflater.Inflate(Resource.Layout.OrderItemLayout, null);
            PurchaseOrderViewModel item = null;
            if (List != null)
                item = List[position];
            else
                return convertView;
            TextView dateTextView = convertView.FindViewById<TextView>(Resource.Id.dateTextView);
            TextView storeNameTextView = convertView.FindViewById<TextView>(Resource.Id.storeNameTextView);
            TextView priceTextView = convertView.FindViewById<TextView>(Resource.Id.priceTextView);
            TextView orderIdTextView = convertView.FindViewById<TextView>(Resource.Id.orderNoTextView);
            TextView orderStatusTextView = convertView.FindViewById<TextView>(Resource.Id.orderStatusTextView);

            convertView.Click += (s, e) =>
             {
                 AnatoliApp.GetInstance().PushFragment(null, "order_detail_fragment", new Tuple<string, string>("AppOrderNo", item.AppOrderNo.ToString()));
             };
            dateTextView.Text = " " + item.OrderPDate;
            storeNameTextView.Text = " " + item.StoreName;
            priceTextView.Text = " " + item.FinalNetAmount.ToCurrency() + " تومان ";
            orderIdTextView.Text = item.AppOrderNo.ToString();
            orderStatusTextView.Text = PurchaseOrderStatusHistoryViewModel.GetStatusName(item.PurchaseOrderStatusValueId);

            return convertView;
        }
    }
}