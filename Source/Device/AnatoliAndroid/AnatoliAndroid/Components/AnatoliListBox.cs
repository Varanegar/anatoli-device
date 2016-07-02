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
using Android.Util;
using Anatoli.Framework.Manager;
using Anatoli.Framework.Model;
using AnatoliAndroid.Activities;

namespace AnatoliAndroid.Components
{
    class AnatoliListBox<DataListAdapter, DataManager, DataModel> : AnatoliTextView
        where DataListAdapter : ListAdapters.BaseListAdapter<DataManager, DataModel>, new()
        where DataModel : BaseModel, new()
        where DataManager : BaseManager<DataModel>, new()
    {
        private new const string Tag = "AnatoliListBox";
        DataListAdapter _listAdapter;
        Dictionary<Guid, DataModel> _dict;
        ListBoxDialog<DataListAdapter, DataManager, DataModel> _listDialog;
        public DataModel SelectedItem { get; private set; }
        
        public void AddItem(DataModel item)
        {
            if (!_dict.ContainsKey(item.UniqueId))
            {
                _listAdapter.List.Add(item);
                _dict.Add(item.UniqueId, item);
            }
        }
        void CreateListBox()
        {
            _listAdapter = new DataListAdapter();
            _dict = new Dictionary<Guid, DataModel>();
            Click += delegate
            {
                if (_listAdapter.List.Count == 0)
                {
                    return;
                }
                _listDialog = new ListBoxDialog<DataListAdapter, DataManager, DataModel>(_listAdapter);
                _listDialog.ItemSelected += (s) =>
                {
                    _listDialog.Dismiss();
                    OnItemSelected(s);
                };
                var t = AnatoliApp.GetInstance().Activity.FragmentManager.BeginTransaction();
                _listDialog.Show(t, "list_dialog");
            };
        }
        public void SelectItem(Guid uniqueId)
        {
            if (_dict.ContainsKey(uniqueId))
            {
                OnItemSelected(_listAdapter.List.Where(i => i == _dict[uniqueId]).FirstOrDefault());
            }
        }
        public void SelectItem(int position)
        {
            if (_listAdapter.List.Count > position)
            {
                OnItemSelected(_listAdapter.List[position]);
            }
        }

        public void SelectItem(DataModel item)
        {
            if (_listAdapter.List.Contains(item))
            {
                OnItemSelected(_listAdapter.List.Where(i => i == item).FirstOrDefault());
            }
        }
        protected AnatoliListBox(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
            CreateListBox();
        }

        public AnatoliListBox(Context context)
            : this(context, null)
        {

        }

        public AnatoliListBox(Context context, IAttributeSet attrs)
            : base(context, attrs, 0)
        {
            CreateListBox();
        }
        void OnItemSelected(DataModel item)
        {
            SelectedItem = item;
            if (item != null)
                Text = item.ToString();
            else
                Text = " - - - ";
            if (ItemSelected != null)
            {
                ItemSelected.Invoke(item);
            }
        }
        public event ItemSelectedEventHandler ItemSelected;
        public delegate void ItemSelectedEventHandler(DataModel item);

        internal void Deselect()
        {
            OnItemSelected(null);
        }

        internal void SetList(List<DataModel> list)
        {
            _listAdapter.List = list;
            _dict = new Dictionary<Guid, DataModel>();
            foreach (var item in list)
            {
                _dict.Add(item.UniqueId, item);
            }
            Deselect();
        }
    }

    class ListBoxDialog<DataListAdapter, DataManager, DataModel> : DialogFragment
        where DataListAdapter : ListAdapters.BaseListAdapter<DataManager, DataModel>, new()
        where DataModel : BaseModel, new()
        where DataManager : BaseManager<DataModel>, new()
    {
        ListView _listView;
        ListAdapters.BaseListAdapter<DataManager, DataModel> _listAdapter;
        public ListBoxDialog(ListAdapters.BaseListAdapter<DataManager, DataModel> listAdapter)
        {
            _listAdapter = listAdapter;
        }
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {

            base.OnCreate(savedInstanceState);
            var view = inflater.Inflate(Resource.Layout.ListBoxLayout, container, false);
            Dialog.Window.RequestFeature(WindowFeatures.NoTitle);
            Dialog.SetCanceledOnTouchOutside(false);

            _listView = view.FindViewById<ListView>(Resource.Id.itemsListView);
            _listView.Adapter = _listAdapter;
            _listView.ItemClick += (s, e) =>
            {
                OnItemSelected(_listAdapter[e.Position]);
            };

            return view;
        }
        void OnItemSelected(DataModel item)
        {
            if (ItemSelected != null)
            {
                ItemSelected.Invoke(item);
            }
        }
        public event ItemSelectedEventHandler ItemSelected;
        public delegate void ItemSelectedEventHandler(DataModel item);
    }
}