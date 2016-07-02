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
using Anatoli.Framework.Model;
using AnatoliAndroid.Activities;
using Anatoli.Framework.Manager;

namespace AnatoliAndroid.ListAdapters
{
    class BaseListAdapter<BaseDataManager, DataModel> : BaseAdapter<DataModel>
        where DataModel : BaseModel, new()
        where BaseDataManager : BaseManager<DataModel>, new()
    {
        public List<DataModel> List;
        protected Activity _context;
        public BaseListAdapter()
        {
            List = new List<DataModel>();
            _context = AnatoliApp.GetInstance().Activity;
        }

        public override int Count
        {
            get { return (List == null) ? 0 : List.Count; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override DataModel this[int position]
        {
            get { return List[position]; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            return GetItemView(position, convertView, parent);
        }
        public virtual View GetItemView(int position, View convertView, ViewGroup parent)
        {
            return convertView;
        }

        public void OnDataChanged()
        {
            if (DataChanged != null)
            {
                DataChanged(this);
            }
        }
        public event DataChangedEventHandler DataChanged;
        public delegate void DataChangedEventHandler(object sender);

    }
}