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
    class OrdersListAdapter : BaseListAdapter<OrderManager, OrderModel>
    {
        public override View GetItemView(int position, View convertView, ViewGroup parent)
        {
            convertView = _context.LayoutInflater.Inflate(Resource.Layout.OrderItemLayout, null);
            OrderModel item = null;
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
                 AnatoliApp.GetInstance().PushFragment<OrderDetailFragment>(null, "order_detail_fragment", new Tuple<string, string>("order_id", item.order_id.ToString()));
             };
            dateTextView.Text = " " + item.order_date;
            storeNameTextView.Text = " " + item.store_name;
            priceTextView.Text = " " + item.order_price.ToCurrency() + " تومان ";
            orderIdTextView.Text = item.order_id.ToString();
            orderStatusTextView.Text = PurchaseOrderStatusHistoryViewModel.GetStatusName(item.order_status);

            return convertView;
        }
    }
}