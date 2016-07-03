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
using Anatoli.Framework.Manager;
using Anatoli.Framework.Model;
using AnatoliAndroid.ListAdapters;
using Anatoli.Framework.AnatoliBase;
using System.Threading.Tasks;
using AnatoliAndroid.Activities;
using AnatoliAndroid.Components;
using Anatoli.Framework;

namespace AnatoliAndroid.Fragments
{
    abstract class BaseListFragment<BaseDataManager, DataListAdapter, DataModel> : AnatoliFragment
        where BaseDataManager : BaseManager<DataModel>, new()
        where DataListAdapter : BaseListAdapter<BaseDataManager, DataModel>, new()
        where DataModel : BaseModel, new()
    {
        protected View _view;
        protected ListView _listView;
        TextView _resultTextView;
        protected DataListAdapter _listAdapter;
        protected BaseDataManager _dataManager;
        public BaseListFragment()
            : base()
        {
            _listAdapter = new DataListAdapter();
            _dataManager = new BaseDataManager();
        }
        protected virtual View InflateLayout(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(
                Resource.Layout.ItemsListLayout, container, false);
            return view;
        }
        public void SetQuery(StringQuery query)
        {
            query.Unlimited = false;
            _dataManager.Query = query;
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            _view = InflateLayout(inflater, container, savedInstanceState);
            _listView = _view.FindViewById<ListView>(Resource.Id.itemsListView);
            _resultTextView = _view.FindViewById<TextView>(Resource.Id.resultTextView);
            _listView.ScrollStateChanged += _listView_ScrollStateChanged;
            _listView.Adapter = _listAdapter;
            EmptyList += (s, e) =>
            {
                _resultTextView.Visibility = ViewStates.Visible;
                _resultTextView.Text = "هیچ موردی یافت نشد";
            };
            FullList += (s, e) =>
            {
                _resultTextView.Visibility = ViewStates.Gone;
            };
            return _view;
        }
        public override void OnStart()
        {
            base.OnStart();
            if (_listView != null)
            {
                Refresh();
            }
        }
        protected void Refresh()
        {
            try
            {
                _listAdapter.List.AddRange(_dataManager.GetNext());
                if (_listAdapter.Count == 0)
                    OnEmptyList();
                else
                    OnFullList();
                _listView.SetSelection(0);
                _listAdapter.NotifyDataSetChanged();
            }
            catch (Exception ex)
            {
                ex.SendTrace();
            }
        }

        async void _listView_ScrollStateChanged(object sender, AbsListView.ScrollStateChangedEventArgs e)
        {
            if (e.ScrollState == ScrollState.Idle)
            {
                if ((_listView.Adapter.Count - 1) <= _listView.LastVisiblePosition)
                {
                    try
                    {
                        await Task.Run(
                            () =>
                                {
                                    var list = _dataManager.GetNext();
                                    if (list.Count > 0)
                                    {
                                        _listAdapter.List.AddRange(list);
                                        _listAdapter.NotifyDataSetChanged();
                                    }
                                }
                            );

                    }
                    catch (Exception ex)
                    {
                        ex.SendTrace();
                    }
                }
            }
        }
        protected void OnEmptyList()
        {
            if (EmptyList != null)
            {
                EmptyList.Invoke(this, new EventArgs());
            }
        }
        public event EventHandler EmptyList;

        protected void OnFullList()
        {
            if (FullList != null)
            {
                FullList.Invoke(this, new EventArgs());
            }
        }
        public event EventHandler FullList;
    }
}