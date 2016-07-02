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
using Anatoli.App.Model;
using Anatoli.App.Model.Store;

namespace AnatoliAndroid.ListAdapters
{
    class DeliveryTypeListAdapter : BaseListAdapter<DeliveryTypeManager,DeliveryTypeModel>
    {
        public override View GetItemView(int position, View convertView, ViewGroup parent)
        {
            convertView = _context.LayoutInflater.Inflate(Resource.Layout.SimpleList1, null);
            DeliveryTypeModel item = null;
            if (List != null)
                item = List[position];
            else
                return convertView;
            TextView textView = convertView.FindViewById<TextView>(Resource.Id.textView);
            textView.Text = item.Name;
            return convertView;
        }
    }
}